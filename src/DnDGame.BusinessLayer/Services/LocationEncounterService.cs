using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Models;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Services;

/// <summary>
/// Application-service implementation of encounter selection. See
/// <see cref="ILocationEncounterService"/> for the enforced rules.
/// </summary>
public sealed class LocationEncounterService : ILocationEncounterService
{
    private readonly ILocationEncounterRepository _locationEncounterRepository;
    private readonly ILocationDefinitionRepository _locationDefinitionRepository;
    private readonly IEnemyRepository _enemyRepository;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IScenarioProgressRepository _scenarioProgressRepository;
    private readonly IPlayerQuestRepository _playerQuestRepository;
    private readonly IBattleRepository _battleRepository;
    private readonly ICurrentPlayerService _currentPlayerService;

    public LocationEncounterService(
        ILocationEncounterRepository locationEncounterRepository,
        ILocationDefinitionRepository locationDefinitionRepository,
        IEnemyRepository enemyRepository,
        IGameSessionRepository gameSessionRepository,
        ICharacterRepository characterRepository,
        IScenarioProgressRepository scenarioProgressRepository,
        IPlayerQuestRepository playerQuestRepository,
        IBattleRepository battleRepository,
        ICurrentPlayerService currentPlayerService)
    {
        _locationEncounterRepository = locationEncounterRepository;
        _locationDefinitionRepository = locationDefinitionRepository;
        _enemyRepository = enemyRepository;
        _gameSessionRepository = gameSessionRepository;
        _characterRepository = characterRepository;
        _scenarioProgressRepository = scenarioProgressRepository;
        _playerQuestRepository = playerQuestRepository;
        _battleRepository = battleRepository;
        _currentPlayerService = currentPlayerService;
    }

    public async Task<EncounterSelectionResult> GetAvailableEncountersAsync(EncounterSelectionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var session = await RequireOwnedSessionAsync(context.GameSessionId);
        var character = await RequireCharacterAsync(session);

        var locationDefinition = await _locationDefinitionRepository.GetByIdAsync(context.LocationId);
        if (locationDefinition is not null && character.Level < locationDefinition.RecommendedMinimumLevel)
        {
            return Empty(context.LocationId);
        }

        var encounterDefinition = await _locationEncounterRepository.GetByLocationIdAsync(context.LocationId);
        if (encounterDefinition is null || encounterDefinition.Enemies.Count == 0)
        {
            return Empty(context.LocationId);
        }

        var enemies = await _enemyRepository.GetAllAsync();
        var enemiesById = enemies.ToDictionary(enemy => enemy.Id);

        var progress = await _scenarioProgressRepository.GetByGameSessionAsync(session.Id);
        var playerQuests = await _playerQuestRepository.GetByGameSessionAsync(session.Id);

        // Scoped by LocationId, not just EnemyId: the same enemy can be a one-time
        // Boss at one location and a repeatable Normal/Elite encounter at another
        // (e.g. Dread Wraith at Whispering Woods vs. Darkstorm Keep) — defeating one
        // must not hide the other. Battles from the story-node pipeline don't set
        // LocationId yet (see Battle.LocationId's remarks) and so never match here.
        var defeatedBossEnemyIds = (await _battleRepository.GetByGameSessionIdAsync(session.Id))
            .Where(battle => battle.Status == GameSessionStatus.Victory && battle.LocationId == context.LocationId)
            .Select(battle => battle.EnemyId)
            .ToHashSet();

        var available = encounterDefinition.Enemies
            .Where(slot => enemiesById.ContainsKey(slot.EnemyId))
            .Where(slot => IsAvailable(slot, context, character, progress, playerQuests, defeatedBossEnemyIds))
            .Select(slot => new EncounterOption(slot.EnemyId, enemiesById[slot.EnemyId].Name, slot.Tier))
            .ToList();

        return new EncounterSelectionResult(context.LocationId, available);
    }

    // --- Gating checks ---

    private static bool IsAvailable(
        EncounterEnemyDefinition slot,
        EncounterSelectionContext context,
        PlayerCharacter character,
        ScenarioProgress? progress,
        IReadOnlyCollection<PlayerQuest> playerQuests,
        IReadOnlySet<int> defeatedBossEnemyIds)
    {
        // Boss encounters are one-time; an NPC that became an ally (see the
        // RequiredStoryFlags checks below) or a boss already defeated this session
        // is never returned again. Normal/Elite encounters stay repeatable.
        if (slot.Tier == EncounterTier.Boss && defeatedBossEnemyIds.Contains(slot.EnemyId))
        {
            return false;
        }

        var requirement = slot.Availability;
        if (requirement is null)
        {
            return true;
        }

        if (requirement.RequiredSubLocations.Count > 0 &&
            (context.SubLocation is null || !requirement.RequiredSubLocations.Contains(context.SubLocation)))
        {
            return false;
        }

        foreach (var (flag, requiredValue) in requirement.RequiredStoryFlags)
        {
            if (!MatchesStoryFlag(progress, flag, requiredValue))
            {
                return false;
            }
        }

        if (requirement.MinimumAshClock is int minimumAshClock && (progress?.AshClock ?? 0) < minimumAshClock)
        {
            return false;
        }

        if (requirement.RequiredActiveQuestId is int requiredQuestId)
        {
            var linkedQuest = playerQuests.FirstOrDefault(pq => pq.QuestId == requiredQuestId);
            if (linkedQuest is not { Status: QuestStatus.Active })
            {
                return false;
            }
        }

        if (requirement.RequiredRace is int requiredRaceId && character.RaceId != requiredRaceId)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Deliberately different from QuestService.HasFlag: that helper treats a
    /// missing ScenarioProgress as an unconditional false match, which is only
    /// correct for requirements that need a flag to be true. This service also uses
    /// requiredValue: false (e.g. "not yet restored"), where a missing flag — a
    /// fresh session with nothing set — must count as satisfied, per the existing
    /// "a missing flag is false" convention (see ChoiceRequirement).
    /// </summary>
    private static bool MatchesStoryFlag(ScenarioProgress? progress, string flag, bool requiredValue)
    {
        var actualValue = progress?.StoryFlags.GetValueOrDefault(flag) ?? false;
        return actualValue == requiredValue;
    }

    private static EncounterSelectionResult Empty(LocationId locationId) =>
        new(locationId, Array.Empty<EncounterOption>());

    // --- Resource resolution (mirrors QuestService's conventions) ---

    private async Task<GameSession> RequireOwnedSessionAsync(int gameSessionId)
    {
        var session = await _gameSessionRepository.GetByIdAsync(gameSessionId);
        if (session is null || session.CharacterId != _currentPlayerService.GetCurrentPlayerId())
        {
            throw new DomainException(ErrorCodes.NotFound, $"Game session {gameSessionId} was not found.");
        }

        return session;
    }

    private async Task<PlayerCharacter> RequireCharacterAsync(GameSession session)
    {
        var character = await _characterRepository.GetByIdAsync(session.CharacterId);
        if (character is null)
        {
            throw new DomainException(
                ErrorCodes.NotFound,
                $"Character {session.CharacterId} belonging to game session {session.Id} was not found.");
        }

        return character;
    }
}
