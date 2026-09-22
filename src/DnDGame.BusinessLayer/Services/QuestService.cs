using System.Globalization;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Models;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Engine.Locations;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Entities.Locations;
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
    private readonly ILocationProgressRepository _locationProgressRepository;
    private readonly ILocationUnlockEngine _locationUnlockEngine;
    private readonly ILocationRouteProvider _locationRouteProvider;
    private readonly IExplicitLocationUnlockService _explicitLocationUnlockService;

    public QuestService(
        IQuestRepository questRepository,
        IPlayerQuestRepository playerQuestRepository,
        IScenarioProgressRepository scenarioProgressRepository,
        IGameSessionRepository gameSessionRepository,
        ICharacterRepository characterRepository,
        IExperienceService experienceService,
        ICurrentPlayerService currentPlayerService,
        ILocationProgressRepository locationProgressRepository,
        ILocationUnlockEngine locationUnlockEngine,
        ILocationRouteProvider locationRouteProvider,
        IExplicitLocationUnlockService explicitLocationUnlockService)
    {
        _questRepository = questRepository;
        _playerQuestRepository = playerQuestRepository;
        _scenarioProgressRepository = scenarioProgressRepository;
        _gameSessionRepository = gameSessionRepository;
        _characterRepository = characterRepository;
        _experienceService = experienceService;
        _currentPlayerService = currentPlayerService;
        _locationProgressRepository = locationProgressRepository;
        _locationUnlockEngine = locationUnlockEngine;
        _locationRouteProvider = locationRouteProvider;
        _explicitLocationUnlockService = explicitLocationUnlockService;
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

    public async Task<IReadOnlyList<QuestProgressView>> GetCompletedQuestsAsync(int gameSessionId)
    {
        var session = await RequireOwnedSessionAsync(gameSessionId);
        var playerQuests = await _playerQuestRepository.GetByGameSessionAsync(session.Id);
        var quests = await _questRepository.GetAllAsync();
        var questById = quests.ToDictionary(quest => quest.Id);

        return playerQuests
            .Where(pq => pq.Status == QuestStatus.Completed)
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

        var rewardEffects = await GrantObjectiveRewardsAsync(session, character, objective, progress);
        progress = rewardEffects.Progress;

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
                ObjectiveCompleted: true, NextObjectiveIndex: nextIndex, Completed: null)
            {
                NewLocationIds = ToIntLocationIds(rewardEffects.NewLocationIds)
            };
        }

        playerQuest.CurrentObjectiveIndex = nextIndex;
        var completion = await FinalizeCompletedQuestAsync(session, character, quest, playerQuest, progress);
        return new UpdateObjectiveResult(
            quest.Id, objective.Id, newProgress, objective.RequiredAmount,
            ObjectiveCompleted: true, NextObjectiveIndex: null, Completed: completion)
        {
            NewLocationIds = ToIntLocationIds(CombineNewLocationIds(
                rewardEffects.NewLocationIds,
                completion.NewLocationIds))
        };
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

    // --- Player-based conveniences ---

    public async Task<IReadOnlyList<QuestView>> GetAvailableQuestsForPlayerAsync(int playerId, int? locationId = null)
    {
        var sessionId = await ResolveSessionIdForPlayerAsync(playerId);
        return await GetAvailableQuestsAsync(sessionId, locationId);
    }

    public async Task<IReadOnlyList<QuestProgressView>> GetActiveQuestsForPlayerAsync(int playerId)
    {
        var sessionId = await ResolveSessionIdForPlayerAsync(playerId);
        return await GetActiveQuestsAsync(sessionId);
    }

    public async Task<IReadOnlyList<QuestProgressView>> GetCompletedQuestsForPlayerAsync(int playerId)
    {
        var sessionId = await ResolveSessionIdForPlayerAsync(playerId);
        return await GetCompletedQuestsAsync(sessionId);
    }

    public async Task<QuestProgressView> StartQuestForPlayerAsync(int playerId, int questId)
    {
        var sessionId = await ResolveSessionIdForPlayerAsync(playerId);
        return await StartQuestAsync(sessionId, questId);
    }

    public async Task<QuestCompletionResult> CompleteQuestForPlayerAsync(int playerId, int questId)
    {
        var sessionId = await ResolveSessionIdForPlayerAsync(playerId);
        return await CompleteQuestAsync(sessionId, questId);
    }

    private async Task<int> ResolveSessionIdForPlayerAsync(int playerId)
    {
        var characters = await _characterRepository.GetAllAsync();
        var character = characters.FirstOrDefault(c => c.OwnerId == playerId.ToString(CultureInfo.InvariantCulture));
        if (character is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"No character was found for player {playerId}.");
        }

        var sessions = await _gameSessionRepository.GetByCharacterIdAsync(character.Id);
        var inProgressSessionId = sessions.FirstOrDefault(s => s.Status == GameSessionStatus.InProgress)?.Id;
        if (inProgressSessionId.HasValue)
        {
            return inProgressSessionId.Value;
        }

        var anySessionId = sessions.FirstOrDefault()?.Id;
        if (anySessionId.HasValue)
        {
            return anySessionId.Value;
        }

        throw new DomainException(ErrorCodes.NotFound, $"No game session was found for player {playerId}.");
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
        var rewardEffects = await ApplyRewardEffectsAsync(session, character, progress, quest.Rewards, quest.ResultFlags);
        progress = rewardEffects.Progress;
        progress = await ApplyOutcomeEffectsAsync(session, progress, quest);
        var derivedNewLocationIds = await EvaluateLocationUnlocksAsync(session, character, progress);
        var newLocationIds = CombineNewLocationIds(rewardEffects.NewLocationIds, derivedNewLocationIds);

        return new QuestCompletionResult(
            quest.Id,
            quest.Title,
            experienceResult.ExperienceGained,
            experienceResult.PreviousLevel,
            experienceResult.CurrentLevel,
            experienceResult.SkillPointsGained,
            newLocationIds);
    }

    private async Task<RewardEffectsResult> GrantObjectiveRewardsAsync(
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

        return await ApplyRewardEffectsAsync(session, character, progress, objective.Rewards);
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
    /// progress record on demand. Also applies explicit authored location unlocks.
    /// </summary>
    private async Task<RewardEffectsResult> ApplyRewardEffectsAsync(
        GameSession session,
        PlayerCharacter character,
        ScenarioProgress? progress,
        IEnumerable<QuestReward> rewards,
        IReadOnlyDictionary<string, bool>? extraFlags = null)
    {
        var rewardList = rewards.ToList();
        var flags = new Dictionary<string, bool>();
        var loyaltyDeltas = new Dictionary<int, int>();
        var warScoreDelta = 0;

        foreach (var reward in rewardList)
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

        if (flags.Count > 0 || loyaltyDeltas.Count > 0 || warScoreDelta != 0)
        {
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
        }

        var newLocationIds = await UnlockExplicitRewardLocationsAsync(character, rewardList);
        return new RewardEffectsResult(progress, newLocationIds);
    }

    private async Task<IReadOnlyList<LocationId>> UnlockExplicitRewardLocationsAsync(
        PlayerCharacter character,
        IReadOnlyCollection<QuestReward> rewards)
    {
        var locationIds = rewards.SelectMany(reward => reward.NewLocationIds);
        return await _explicitLocationUnlockService.UnlockExplicitLocationsAsync(character, locationIds);
    }

    private static IReadOnlyList<LocationId> CombineNewLocationIds(
        params IEnumerable<LocationId>[] locationIdGroups)
    {
        var result = new List<LocationId>();
        var seen = new HashSet<LocationId>();

        foreach (var group in locationIdGroups)
        {
            foreach (var locationId in group)
            {
                if (seen.Add(locationId))
                {
                    result.Add(locationId);
                }
            }
        }

        return result;
    }

    private static IReadOnlyCollection<int> ToIntLocationIds(IEnumerable<LocationId> locationIds)
    {
        return locationIds.Select(locationId => (int)locationId).Distinct().ToList();
    }

    private sealed record RewardEffectsResult(
        ScenarioProgress? Progress,
        IReadOnlyList<LocationId> NewLocationIds);

    /// <summary>
    /// Quest.Outcomes is alternative-resolution configuration (see QuestOutcome's own
    /// remarks: "applying rewards, flags and effects belongs to quest logic") that,
    /// until BACK-LOC-06, nothing ever read — completing a quest with Outcomes
    /// silently applied none of them. This selects the first outcome whose
    /// RequiredFlags all currently match (the usual "missing flag counts as false"
    /// convention — see MatchesRequiredFlag, deliberately not the stricter HasFlag
    /// used elsewhere in this class, which would wrongly reject a false-requiring
    /// outcome on a session with no ScenarioProgress yet) and merges only its
    /// ResultFlags and Allies into StoryFlags — Allies keyed by their own stable ally
    /// code (e.g. "divine-chimera"), so "was this NPC recruited" is just an ordinary
    /// story flag, queryable the same way any other one is (e.g. by
    /// LocationEncounterService to exclude that NPC's Enemy row once true).
    ///
    /// Deliberately NOT applied here: ExperienceRewardPercentage, WarScoreChange,
    /// AshClockChange, CorruptionChange, CompanionLoyaltyChanges, CounterChanges,
    /// Items. Wiring those needs design decisions (e.g. a companion code-to-id
    /// lookup that doesn't exist yet) outside BACK-LOC-06's scope.
    /// </summary>
    private async Task<ScenarioProgress?> ApplyOutcomeEffectsAsync(GameSession session, ScenarioProgress? progress, Quest quest)
    {
        var eligibleOutcome = quest.Outcomes.FirstOrDefault(outcome =>
            outcome.RequiredFlags.All(entry => MatchesRequiredFlag(progress, entry.Key, entry.Value)));
        if (eligibleOutcome is null)
        {
            return progress;
        }

        var flags = new Dictionary<string, bool>();
        foreach (var (flag, value) in eligibleOutcome.ResultFlags)
        {
            flags[flag] = value;
        }

        foreach (var (allyCode, recruited) in eligibleOutcome.Allies)
        {
            flags[allyCode] = recruited;
        }

        if (flags.Count == 0)
        {
            return progress;
        }

        var created = progress is null;
        progress ??= new ScenarioProgress { GameSessionId = session.Id };

        foreach (var (flag, value) in flags)
        {
            progress.StoryFlags[flag] = value;
        }

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

    /// <summary>
    /// Unlike HasFlag (which treats a missing ScenarioProgress as an unconditional
    /// false match, correct only for flags required to be true), this treats a
    /// missing flag as false regardless of which value is required — the same
    /// "a missing flag is false" convention documented on ChoiceRequirement — so an
    /// outcome whose RequiredFlags calls for a flag to be false still matches on a
    /// fresh session where nothing has been set yet.
    /// </summary>
    private static bool MatchesRequiredFlag(ScenarioProgress? progress, string flag, bool requiredValue)
    {
        var actualValue = progress?.StoryFlags.GetValueOrDefault(flag) ?? false;
        return actualValue == requiredValue;
    }

    // --- Location unlocks (BACK-LOC-07) ---

    // MQ-01/MQ-02/MQ-06/MQ-07/MQ-08/MQ-09 have no gating role inside
    // ILocationUnlockEngine itself (it only knows about quests 3/6/10/13 — see its
    // own private constants); they are how THIS class derives the inputs the engine
    // cannot compute on its own (CompletedLocationIds and CrownFragmentCount).
    // HeroOverlookFinaleQuestId duplicates the engine's own private constant
    // deliberately — mirroring it here (rather than exposing it from the engine) is
    // what keeps QuestService a caller of the engine instead of a dependency of it,
    // avoiding a circular reference.
    private const int PrologueQuestId = 1;
    private const int RacialRouteQuestId = 2;

    // "The Unburned Town" is Oakheaven's own resolution quest (Mage/Warrior/Bard/
    // Healer solution) — its completion is what marks Oakheaven itself Completed.
    // It's also ILocationUnlockEngine's FragmentRouteFlexQuestId, which is what
    // actually unlocks the three flexible fragment locations; this second effect
    // only keeps LocationProgress.Completed accurate for Oakheaven, it changes
    // nothing about which locations become available.
    private const int OakheavenResolvedQuestId = 6;

    private static readonly int[] CrownFragmentQuestIds = { 7, 8, 9 };
    private const int HeroOverlookFinaleQuestId = 13;

    // Matches LocationDefinition.SpecialFlags/LocationEncounterSeedData's own gate
    // for Hero's Overlook's final bosses (BACK-LOC-06) — setting it here is what
    // makes MQ-13's "final enemies stop being locked to the prologue" rule live.
    private const string FinaleUnlockedFlag = "FinaleUnlocked";

    /// <summary>
    /// Re-evaluates which locations are newly available to this player after a
    /// quest completes, persists that into LocationProgress, and returns the
    /// locations that just became available. ILocationUnlockEngine already encodes
    /// every gating rule (guild registration by MQ-03, three fragments + MQ-10 for
    /// Darkstorm Keep, MQ-13 for the finale, racial-route ordering) — this method's
    /// only job is feeding it accurate state; it never duplicates or overrides the
    /// engine's own unlock logic.
    /// </summary>
    private async Task<IReadOnlyList<LocationId>> EvaluateLocationUnlocksAsync(
        GameSession session, PlayerCharacter character, ScenarioProgress? progress)
    {
        if (!Enum.IsDefined(typeof(RaceType), character.RaceId))
        {
            // No route is configured for this RaceId (e.g. test data outside 1-4).
            // This is a supplementary step, not core to quest completion, so skip
            // rather than fail the whole completion over unrelated location data.
            return Array.Empty<LocationId>();
        }

        var playerQuests = await _playerQuestRepository.GetByGameSessionAsync(session.Id);
        var completedQuestIds = playerQuests
            .Where(pq => pq.Status == QuestStatus.Completed)
            .Select(pq => pq.QuestId)
            .ToHashSet();

        var existingProgress = await _locationProgressRepository.GetByPlayerIdAsync(character.Id);
        var progressByLocation = existingProgress.ToDictionary(p => p.LocationId);

        var unlockedLocationIds = progressByLocation.Values
            .Where(p => p.Status != LocationStatus.Locked)
            .Select(p => p.LocationId)
            .ToList();

        var completedLocationIds = progressByLocation.Values
            .Where(p => p.Completed)
            .Select(p => p.LocationId)
            .ToHashSet();

        var race = (RaceType)character.RaceId;
        var route = _locationRouteProvider.GetRecommendedRoute(race);
        var raceFirstLocationId = route.Steps.OrderBy(step => step.Order).Skip(1).First().LocationId;

        // No other system currently calls ILocationUnlockEngine.CompleteLocation, so
        // these are the only ways a location's completion is ever recorded: MQ-01
        // finishing the prologue, MQ-02 ("The Road That Is Yours") finishing the
        // player's race-specific opening, MQ-06 ("The Unburned Town") resolving
        // Oakheaven itself.
        if (completedQuestIds.Contains(PrologueQuestId))
        {
            completedLocationIds.Add(LocationId.HerosOverlook);
        }

        if (completedQuestIds.Contains(RacialRouteQuestId))
        {
            completedLocationIds.Add(raceFirstLocationId);
        }

        if (completedQuestIds.Contains(OakheavenResolvedQuestId))
        {
            completedLocationIds.Add(LocationId.Oakheaven);
        }

        var context = new LocationUnlockContext
        {
            PlayerId = character.Id,
            Race = race,
            Level = character.Level,
            CompletedQuestIds = completedQuestIds.ToList(),
            StoryFlags = progress?.StoryFlags ?? new Dictionary<string, bool>(),
            CrownFragmentCount = CrownFragmentQuestIds.Count(completedQuestIds.Contains),
            CompletedLocationIds = completedLocationIds.ToList(),
            UnlockedLocationIds = unlockedLocationIds
        };

        var availableResult = _locationUnlockEngine.GetAvailableLocations(context);
        if (!availableResult.Success || availableResult.Data is null)
        {
            return Array.Empty<LocationId>();
        }

        var newLocationIds = availableResult.Data
            .Where(locationId => !unlockedLocationIds.Contains(locationId))
            .ToList();

        // The engine deliberately re-surfaces Hero's Overlook once MQ-13 completes
        // (the Finale), even though it was already unlocked from the prologue — so
        // it never appears via the plain "not already unlocked" diff above.
        if (completedQuestIds.Contains(HeroOverlookFinaleQuestId))
        {
            var finaleAlreadyUnlocked = progress?.StoryFlags.GetValueOrDefault(FinaleUnlockedFlag) ?? false;
            if (!finaleAlreadyUnlocked)
            {
                progress = await SetStoryFlagAsync(session, progress, FinaleUnlockedFlag, true);
                if (!newLocationIds.Contains(LocationId.HerosOverlook))
                {
                    newLocationIds.Add(LocationId.HerosOverlook);
                }
            }
        }

        foreach (var locationId in newLocationIds)
        {
            await UpsertLocationProgressAsync(progressByLocation, character, locationId, completed: false);
        }

        foreach (var locationId in completedLocationIds)
        {
            await UpsertLocationProgressAsync(progressByLocation, character, locationId, completed: true);
        }

        return newLocationIds;
    }

    private async Task<ScenarioProgress> SetStoryFlagAsync(GameSession session, ScenarioProgress? progress, string flag, bool value)
    {
        var created = progress is null;
        progress ??= new ScenarioProgress { GameSessionId = session.Id };
        progress.StoryFlags[flag] = value;

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

    private async Task UpsertLocationProgressAsync(
        Dictionary<LocationId, LocationProgress> progressByLocation,
        PlayerCharacter character,
        LocationId locationId,
        bool completed)
    {
        if (progressByLocation.TryGetValue(locationId, out var existing))
        {
            var changed = false;
            if (existing.Status == LocationStatus.Locked)
            {
                existing.Status = LocationStatus.Available;
                existing.UnlockedAtLevel ??= character.Level;
                changed = true;
            }

            if (completed && !existing.Completed)
            {
                existing.Completed = true;
                changed = true;
            }

            if (changed)
            {
                await _locationProgressRepository.UpdateAsync(existing);
            }

            return;
        }

        var created = new LocationProgress
        {
            PlayerId = character.Id,
            LocationId = locationId,
            Status = LocationStatus.Available,
            UnlockedAtLevel = character.Level,
            Completed = completed
        };
        await _locationProgressRepository.AddAsync(created);
        progressByLocation[locationId] = created;
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
