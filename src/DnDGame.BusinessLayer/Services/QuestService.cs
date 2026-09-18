using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Models;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Services;

/// <summary>
/// Application-service implementation of quest management. See
/// <see cref="IQuestService"/> for the enforced rules and error conventions.
/// </summary>
public sealed class QuestService : IQuestService
{
    private readonly IQuestRepository _questRepository;
    private readonly IPlayerQuestRepository _playerQuestRepository;
    private readonly IScenarioProgressRepository _scenarioProgressRepository;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IExperienceService _experienceService;
    private readonly ICurrentPlayerService _currentPlayerService;

    public QuestService(
        IQuestRepository questRepository,
        IPlayerQuestRepository playerQuestRepository,
        IScenarioProgressRepository scenarioProgressRepository,
        IGameSessionRepository gameSessionRepository,
        ICharacterRepository characterRepository,
        IExperienceService experienceService,
        ICurrentPlayerService currentPlayerService)
    {
        _questRepository = questRepository;
        _playerQuestRepository = playerQuestRepository;
        _scenarioProgressRepository = scenarioProgressRepository;
        _gameSessionRepository = gameSessionRepository;
        _characterRepository = characterRepository;
        _experienceService = experienceService;
        _currentPlayerService = currentPlayerService;
    }

    public async Task<IReadOnlyList<QuestView>> GetAvailableQuestsAsync(
        int gameSessionId,
        int? locationId = null)
    {
        var session = await RequireOwnedSessionAsync(gameSessionId);
        var character = await RequireCharacterAsync(session);
        var quests = await _questRepository.GetAllAsync();
        var playerQuests = await _playerQuestRepository.GetByGameSessionAsync(session.Id);
        var progress = await _scenarioProgressRepository.GetByGameSessionAsync(session.Id);

        return quests
            .Where(quest => IsOfferable(quest, character, locationId, playerQuests, progress))
            .OrderBy(quest => quest.QuestType == QuestType.Main ? 0 : 1)
            .ThenBy(quest => quest.RecommendedLevel)
            .ThenBy(quest => quest.Id)
            .Select(ToQuestView)
            .ToList();
    }

    public async Task<IReadOnlyList<QuestProgressView>> GetActiveQuestsAsync(int gameSessionId)
    {
        var session = await RequireOwnedSessionAsync(gameSessionId);
        var playerQuests = await _playerQuestRepository.GetByGameSessionAsync(session.Id);
        var quests = await _questRepository.GetAllAsync();
        var questById = quests.ToDictionary(quest => quest.Id);

        return playerQuests
            .Where(pq => pq.Status == QuestStatus.Active)
            .Where(pq => questById.ContainsKey(pq.QuestId))
            .Select(pq => ToProgressView(questById[pq.QuestId], pq))
            .OrderBy(view => view.QuestId)
            .ToList();
    }

    public async Task<QuestView> GetQuestByIdAsync(int questId)
    {
        var quest = await RequireQuestAsync(questId);
        return ToQuestView(quest);
    }

    public async Task<QuestProgressView> StartQuestAsync(int gameSessionId, int questId)
    {
        var session = await RequireOwnedSessionAsync(gameSessionId);
        var character = await RequireCharacterAsync(session);
        var quest = await RequireQuestAsync(questId);
        var playerQuests = await _playerQuestRepository.GetByGameSessionAsync(session.Id);
        var progress = await _scenarioProgressRepository.GetByGameSessionAsync(session.Id);

        var existing = playerQuests.FirstOrDefault(pq => pq.QuestId == questId);
        if (existing is { Status: QuestStatus.Active or QuestStatus.Completed or QuestStatus.Failed })
        {
            throw new DomainException(
                ErrorCodes.Conflict,
                $"Quest '{quest.Title}' cannot be started again — it is already {existing.Status}.");
        }

        if (!MeetsLevelGate(quest, character) ||
            !MeetsPrerequisites(quest, character, playerQuests, progress))
        {
            throw new DomainException(
                ErrorCodes.Conflict,
                $"Quest '{quest.Title}' cannot be started — its prerequisites are not met.");
        }

        if (existing is null)
        {
            existing = new PlayerQuest
            {
                GameSessionId = session.Id,
                QuestId = questId,
                Status = QuestStatus.Active,
                CurrentObjectiveIndex = 0,
                StartedAt = DateTime.UtcNow
            };
            await _playerQuestRepository.AddAsync(existing);
        }
        else
        {
            existing.Status = QuestStatus.Active;
            existing.StartedAt ??= DateTime.UtcNow;
            await _playerQuestRepository.UpdateAsync(existing);
        }

        return ToProgressView(quest, existing);
    }

    public async Task<UpdateObjectiveResult> UpdateObjectiveAsync(
        int gameSessionId,
        int questId,
        int objectiveId,
        int amount = 1)
    {
        if (amount <= 0)
        {
            throw new DomainException(ErrorCodes.ValidationError, "amount must be a positive integer.");
        }

        var session = await RequireOwnedSessionAsync(gameSessionId);
        var character = await RequireCharacterAsync(session);
        var quest = await RequireQuestAsync(questId);
        var playerQuest = await RequirePlayerQuestAsync(quest, gameSessionId);

        EnsureActive(playerQuest);

        if (quest.Objectives.Count == 0)
        {
            throw new DomainException(ErrorCodes.Conflict, $"Quest '{quest.Title}' has no objectives.");
        }

        var objectiveIndex = playerQuest.CurrentObjectiveIndex;
        if (objectiveIndex >= quest.Objectives.Count)
        {
            throw new DomainException(ErrorCodes.Conflict, $"Quest '{quest.Title}' has no remaining objectives.");
        }

        var objective = quest.Objectives[objectiveIndex];
        if (objective.Id != objectiveId)
        {
            throw new DomainException(
                ErrorCodes.Conflict,
                $"Objective {objectiveId} is not the current objective of '{quest.Title}'; the current objective is {objective.Id}.");
        }

        var progress = await _scenarioProgressRepository.GetByGameSessionAsync(session.Id);

        playerQuest.ObjectiveProgress.TryGetValue(objective.Id, out var currentProgress);
        var newProgress = currentProgress + amount;
        playerQuest.ObjectiveProgress[objective.Id] = newProgress;

        if (newProgress < objective.RequiredAmount)
        {
            await _playerQuestRepository.UpdateAsync(playerQuest);
            return new UpdateObjectiveResult(
                quest.Id, objective.Id, newProgress, objective.RequiredAmount,
                ObjectiveCompleted: false, NextObjectiveIndex: objectiveIndex, Completed: null);
        }

        progress = await GrantObjectiveRewardsAsync(session, character, objective, progress);

        var nextIndex = objectiveIndex + 1;
        while (nextIndex < quest.Objectives.Count && quest.Objectives[nextIndex].IsOptional)
        {
            nextIndex++;
        }

        if (nextIndex < quest.Objectives.Count)
        {
            playerQuest.CurrentObjectiveIndex = nextIndex;
            await _playerQuestRepository.UpdateAsync(playerQuest);
            return new UpdateObjectiveResult(
                quest.Id, objective.Id, newProgress, objective.RequiredAmount,
                ObjectiveCompleted: true, NextObjectiveIndex: nextIndex, Completed: null);
        }

        playerQuest.CurrentObjectiveIndex = nextIndex;
        var completion = await FinalizeCompletedQuestAsync(session, character, quest, playerQuest, progress);
        return new UpdateObjectiveResult(
            quest.Id, objective.Id, newProgress, objective.RequiredAmount,
            ObjectiveCompleted: true, NextObjectiveIndex: null, Completed: completion);
    }

    public async Task<QuestCompletionResult> CompleteQuestAsync(int gameSessionId, int questId)
    {
        var session = await RequireOwnedSessionAsync(gameSessionId);
        var character = await RequireCharacterAsync(session);
        var quest = await RequireQuestAsync(questId);
        var playerQuest = await RequirePlayerQuestAsync(quest, gameSessionId);

        EnsureActive(playerQuest);

        var progress = await _scenarioProgressRepository.GetByGameSessionAsync(session.Id);
        return await FinalizeCompletedQuestAsync(session, character, quest, playerQuest, progress);
    }

    public async Task FailQuestAsync(int gameSessionId, int questId)
    {
        var session = await RequireOwnedSessionAsync(gameSessionId);
        await RequireCharacterAsync(session);
        var quest = await RequireQuestAsync(questId);
        var playerQuest = await RequirePlayerQuestAsync(quest, gameSessionId);

        EnsureActive(playerQuest);

        playerQuest.Status = QuestStatus.Failed;
        await _playerQuestRepository.UpdateAsync(playerQuest);
    }

    // --- Grand operations shared by the public completion paths ---

    /// <summary>
    /// Performs the terminal transition Active -> Completed, grants the quest's
    /// experience exactly once and applies its reward + success flags. No guard is
    /// checked here — callers must already have confirmed the quest is Active.
    /// </summary>
    private async Task<QuestCompletionResult> FinalizeCompletedQuestAsync(
        GameSession session,
        PlayerCharacter character,
        Quest quest,
        PlayerQuest playerQuest,
        ScenarioProgress? progress)
    {
        playerQuest.Status = QuestStatus.Completed;
        playerQuest.CompletedAt = DateTime.UtcNow;
        await _playerQuestRepository.UpdateAsync(playerQuest);

        var experienceResult = await GrantExperienceAsync(character, quest.Rewards.Sum(r => r.Experience));
        await ApplyRewardEffectsAsync(session, progress, quest.Rewards, quest.ResultFlags);

        return new QuestCompletionResult(
            quest.Id,
            quest.Title,
            experienceResult.ExperienceGained,
            experienceResult.PreviousLevel,
            experienceResult.CurrentLevel,
            experienceResult.SkillPointsGained);
    }

    private async Task<ScenarioProgress?> GrantObjectiveRewardsAsync(
        GameSession session,
        PlayerCharacter character,
        QuestObjective objective,
        ScenarioProgress? progress)
    {
        var objectiveExperience = objective.Rewards.Sum(reward => reward.Experience);
        if (objectiveExperience > 0)
        {
            await GrantExperienceAsync(character, objectiveExperience);
        }

        return await ApplyRewardEffectsAsync(session, progress, objective.Rewards);
    }

    private async Task<ExperienceResult> GrantExperienceAsync(PlayerCharacter character, int experience)
    {
        var result = _experienceService.AddExperience(character, experience);
        await _characterRepository.UpdateAsync(character);
        return result;
    }

    /// <summary>
    /// Merges reward story flags/loyalty/war score (plus optional extra flags such
    /// as the quest's ResultFlags) into the session's ScenarioProgress, creating the
    /// progress record on demand. Returns the effective progress instance.
    /// </summary>
    private async Task<ScenarioProgress?> ApplyRewardEffectsAsync(
        GameSession session,
        ScenarioProgress? progress,
        IEnumerable<QuestReward> rewards,
        IReadOnlyDictionary<string, bool>? extraFlags = null)
    {
        var flags = new Dictionary<string, bool>();
        var loyaltyDeltas = new Dictionary<int, int>();
        var warScoreDelta = 0;

        foreach (var reward in rewards)
        {
            foreach (var (flag, value) in reward.StoryFlags)
            {
                flags[flag] = value;
            }

            foreach (var (companionId, delta) in reward.CompanionLoyalty)
            {
                if (delta != 0)
                {
                    loyaltyDeltas.TryGetValue(companionId, out var current);
                    loyaltyDeltas[companionId] = current + delta;
                }
            }

            warScoreDelta += reward.WarScore;
        }

        if (extraFlags is not null)
        {
            foreach (var (flag, value) in extraFlags)
            {
                flags[flag] = value;
            }
        }

        if (flags.Count == 0 && loyaltyDeltas.Count == 0 && warScoreDelta == 0)
        {
            return progress;
        }

        var created = progress is null;
        progress ??= new ScenarioProgress { GameSessionId = session.Id };

        foreach (var (flag, value) in flags)
        {
            progress.StoryFlags[flag] = value;
        }

        foreach (var (companionId, delta) in loyaltyDeltas)
        {
            progress.CompanionLoyalty.TryGetValue(companionId, out var current);
            progress.CompanionLoyalty[companionId] = Math.Max(0, current + delta);
        }

        progress.WarScore = Math.Max(0, progress.WarScore + warScoreDelta);

        if (created)
        {
            await _scenarioProgressRepository.AddAsync(progress);
        }
        else
        {
            await _scenarioProgressRepository.UpdateAsync(progress);
        }

        return progress;
    }

    // --- Gating checks ---

    private bool IsOfferable(
        Quest quest,
        PlayerCharacter character,
        int? locationId,
        IReadOnlyCollection<PlayerQuest> playerQuests,
        ScenarioProgress? progress)
    {
        var playerQuest = playerQuests.FirstOrDefault(pq => pq.QuestId == quest.Id);
        if (playerQuest is { Status: QuestStatus.Active or QuestStatus.Completed or QuestStatus.Failed })
        {
            return false;
        }

        if (!MeetsLevelGate(quest, character) ||
            !PassesLocationGate(quest, locationId) ||
            !MeetsPrerequisites(quest, character, playerQuests, progress))
        {
            return false;
        }

        return true;
    }

    private static bool MeetsLevelGate(Quest quest, PlayerCharacter character)
    {
        if (character.Level < quest.RecommendedLevel)
        {
            return false;
        }

        return quest.RecommendedMaximumLevel is not int maximum || character.Level <= maximum;
    }

    private static bool PassesLocationGate(Quest quest, int? locationId)
    {
        if (locationId is not int location)
        {
            return true;
        }

        if (quest.LocationId == location)
        {
            return true;
        }

        if (quest.PossibleLocationIds.Contains(location))
        {
            return true;
        }

        // Route-dependent quest with no primary location is available anywhere.
        return quest.LocationId is null && quest.PossibleLocationIds.Count == 0;
    }

    private static bool MeetsPrerequisites(
        Quest quest,
        PlayerCharacter character,
        IReadOnlyCollection<PlayerQuest> playerQuests,
        ScenarioProgress? progress)
    {
        foreach (var (flag, value) in quest.RequiredFlags)
        {
            if (!HasFlag(progress, flag, value))
            {
                return false;
            }
        }

        foreach (var prerequisite in quest.Prerequisites)
        {
            if (prerequisite.RequiredQuestId is int requiredQuestId)
            {
                var prerequisiteQuest = playerQuests.FirstOrDefault(pq => pq.QuestId == requiredQuestId);
                if (prerequisiteQuest is null || prerequisiteQuest.Status != prerequisite.RequiredQuestStatus)
                {
                    return false;
                }
            }

            if (prerequisite.MinimumLevel is int minimumLevel && character.Level < minimumLevel)
            {
                return false;
            }

            if (prerequisite.RequiredFlag is not null &&
                !HasFlag(progress, prerequisite.RequiredFlag, prerequisite.RequiredFlagValue))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasFlag(ScenarioProgress? progress, string flag, bool requiredValue)
    {
        if (progress is null)
        {
            return false;
        }

        return progress.StoryFlags.TryGetValue(flag, out var value) && value == requiredValue;
    }

    // --- Resource resolution ---

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

    private async Task<Quest> RequireQuestAsync(int questId)
    {
        var quest = await _questRepository.GetByIdAsync(questId);
        if (quest is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Quest {questId} was not found.");
        }

        return quest;
    }

    private async Task<PlayerQuest> RequirePlayerQuestAsync(Quest quest, int gameSessionId)
    {
        var playerQuest = await _playerQuestRepository.GetByQuestAndSessionAsync(quest.Id, gameSessionId);
        if (playerQuest is null)
        {
            throw new DomainException(
                ErrorCodes.Conflict,
                $"Quest '{quest.Title}' has not been started in game session {gameSessionId}.");
        }

        return playerQuest;
    }

    private void EnsureActive(PlayerQuest playerQuest)
    {
        if (playerQuest.Status != QuestStatus.Active)
        {
            var message = playerQuest.Status == QuestStatus.Completed
                ? "This quest has already been completed and its rewards granted; it cannot be completed again."
                : $"The quest is already {playerQuest.Status}; only an active quest can complete or fail.";
            throw new DomainException(ErrorCodes.Conflict, message);
        }
    }

    // --- Projection helpers ---

    private static QuestView ToQuestView(Quest quest)
    {
        return new QuestView(
            quest.Id,
            quest.Code,
            quest.Title,
            quest.Description,
            quest.QuestType,
            quest.QuestGiver,
            quest.RecommendedLevel,
            quest.RecommendedMaximumLevel,
            quest.LocationId,
            quest.PossibleLocationIds.ToList(),
            quest.IsOptional,
            quest.ExperienceReward);
    }

    private static QuestProgressView ToProgressView(Quest quest, PlayerQuest playerQuest)
    {
        var currentObjective = playerQuest.CurrentObjectiveIndex < quest.Objectives.Count
            ? quest.Objectives[playerQuest.CurrentObjectiveIndex]
            : null;

        return new QuestProgressView(
            playerQuest.QuestId,
            quest.Code,
            quest.Title,
            playerQuest.Status,
            playerQuest.CurrentObjectiveIndex,
            currentObjective?.Id,
            currentObjective?.Description,
            new Dictionary<int, int>(playerQuest.ObjectiveProgress),
            playerQuest.StartedAt,
            playerQuest.CompletedAt);
    }
}