using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Engine.Locations;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using DnDGame.MockData.Services;
using Xunit;

namespace DnDGame.Tests.BusinessLayer;

public class QuestServiceTests
{
    private const int CurrentPlayerId = 1;

    // --- Availability: main quest chain order ---

    [Fact]
    public async Task GetAvailableQuests_OnlyOffersTheNextMainQuestInTheChain()
    {
        var (service, store, _, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");
        var secondMainQuestId = QuestId(store, "MQ-02");

        var available = await service.GetAvailableQuestsAsync(1, locationId: 3);

        Assert.Contains(available, quest => quest.Id == firstMainQuestId);
        Assert.DoesNotContain(available, quest => quest.Id == secondMainQuestId);
    }

    [Fact]
    public async Task GetAvailableQuests_CompletingAMainQuestUnlocksTheNextOne()
    {
        var (service, store, _, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");
        var secondMainQuestId = QuestId(store, "MQ-02");

        await service.StartQuestAsync(1, firstMainQuestId);
        await service.CompleteQuestAsync(1, firstMainQuestId);

        var available = await service.GetAvailableQuestsAsync(1);

        Assert.Contains(available, quest => quest.Id == secondMainQuestId);
        Assert.DoesNotContain(available, quest => quest.Id == firstMainQuestId);
    }

    // --- Availability: location filtering ---

    [Fact]
    public async Task GetAvailableQuests_FiltersByLocation()
    {
        var (service, store, _, _, _) = CreateScenario();
        var questA = AddSyntheticQuest(store, 501, locationId: 1);
        var questB = AddSyntheticQuest(store, 502, locationId: 2);
        var routeDependent = AddSyntheticQuest(store, 503, locationId: null);

        var atLocationOne = await service.GetAvailableQuestsAsync(1, locationId: 1);
        var atLocationTwo = await service.GetAvailableQuestsAsync(1, locationId: 2);

        Assert.Contains(atLocationOne, quest => quest.Id == questA.Id);
        Assert.DoesNotContain(atLocationOne, quest => quest.Id == questB.Id);
        Assert.Contains(atLocationOne, quest => quest.Id == routeDependent.Id);

        Assert.Contains(atLocationTwo, quest => quest.Id == questB.Id);
        Assert.DoesNotContain(atLocationTwo, quest => quest.Id == questA.Id);
    }

    // --- Availability: level filtering ---

    [Fact]
    public async Task GetAvailableQuests_FiltersByLevelRange()
    {
        var (service, store, _, _, _) = CreateScenario(characterLevel: 3);
        var withinRange = AddSyntheticQuest(store, 504, recommendedLevel: 1, recommendedMaximumLevel: 3);
        var tooAdvanced = AddSyntheticQuest(store, 505, recommendedLevel: 5, recommendedMaximumLevel: 6);
        var beyondRecommendedMaximum = AddSyntheticQuest(store, 506, recommendedLevel: 1, recommendedMaximumLevel: 2);

        var available = await service.GetAvailableQuestsAsync(1);

        Assert.Contains(available, quest => quest.Id == withinRange.Id);
        Assert.DoesNotContain(available, quest => quest.Id == tooAdvanced.Id);
        Assert.DoesNotContain(available, quest => quest.Id == beyondRecommendedMaximum.Id);
    }

    // --- Availability: story-flag prerequisites ---

    [Fact]
    public async Task GetAvailableQuests_RespectsRequiredStoryFlags()
    {
        var (service, store, _, _, _) = CreateScenario();
        var gated = AddSyntheticQuest(store, 507, requiredFlags: new Dictionary<string, bool> { ["freed_scout"] = true });

        var before = await service.GetAvailableQuestsAsync(1);
        Assert.DoesNotContain(before, quest => quest.Id == gated.Id);

        store.ScenarioProgresses.Add(new ScenarioProgress
        {
            Id = 1,
            GameSessionId = 1,
            StoryFlags = new Dictionary<string, bool> { ["freed_scout"] = true }
        });

        var after = await service.GetAvailableQuestsAsync(1);
        Assert.Contains(after, quest => quest.Id == gated.Id);
    }

    // --- Starting quests ---

    [Fact]
    public async Task StartQuest_ActivatesQuestAndListsItAsActive()
    {
        var (service, store, _, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");

        var started = await service.StartQuestAsync(1, firstMainQuestId);

        Assert.Equal(QuestStatus.Active, started.Status);
        Assert.NotNull(started.StartedAt);
        Assert.Single(await service.GetActiveQuestsAsync(1), quest => quest.QuestId == firstMainQuestId);
    }

    [Fact]
    public async Task StartQuest_SkipsAheadInTheMainChain_Fails()
    {
        var (service, store, _, _, _) = CreateScenario();
        var secondMainQuestId = QuestId(store, "MQ-02");

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.StartQuestAsync(1, secondMainQuestId));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    [Fact]
    public async Task StartQuest_CompletedQuestCannotBeStartedAgain()
    {
        var (service, store, _, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");
        await service.StartQuestAsync(1, firstMainQuestId);
        await service.CompleteQuestAsync(1, firstMainQuestId);

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.StartQuestAsync(1, firstMainQuestId));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    // --- Completion and EXP ---

    [Fact]
    public async Task CompleteQuest_GrantsExperienceExactlyOnce()
    {
        var (service, store, player, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");
        await service.StartQuestAsync(1, firstMainQuestId);

        var completion = await service.CompleteQuestAsync(1, firstMainQuestId);

        Assert.Equal(100, completion.ExperienceGained);
        Assert.Empty(completion.NewLocationIds);
        Assert.Equal(100, player.CurrentXp);

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.CompleteQuestAsync(1, firstMainQuestId));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
        Assert.Equal(100, player.CurrentXp);
    }

    [Fact]
    public async Task CompleteQuest_LevelUp_ReportsSkillPointsGained()
    {
        var (service, store, player, _, _) = CreateScenario(xp: 0);
        var quest = AddSyntheticQuest(store, 508, experience: 120);
        await service.StartQuestAsync(1, quest.Id);

        var completion = await service.CompleteQuestAsync(1, quest.Id);

        Assert.True(completion.DidLevelUp);
        Assert.Equal(1, completion.PreviousLevel);
        Assert.Equal(2, completion.CurrentLevel);
        Assert.Equal(3, completion.SkillPointsGained);
        Assert.Equal(3, player.SkillPoints);
    }

    [Fact]
    public async Task CompleteQuest_AppliesSuccessStoryFlags()
    {
        var (service, store, _, _, _) = CreateScenario();
        var quest = AddSyntheticQuest(
            store, 509,
            resultFlags: new Dictionary<string, bool> { ["side_complete"] = true });

        await service.StartQuestAsync(1, quest.Id);
        await service.CompleteQuestAsync(1, quest.Id);

        var progress = Assert.Single(store.ScenarioProgresses);
        Assert.Equal(1, progress.GameSessionId);
        Assert.True(progress.StoryFlags["side_complete"]);
    }

    [Fact]
    public async Task CompleteQuest_FirstExplicitLocationUnlock_PersistsAndReturnsIt()
    {
        var (service, store, _, _, _) = CreateScenario();
        var quest = AddSyntheticQuest(store, 510, newLocationIds: new[] { 4 });

        await service.StartQuestAsync(1, quest.Id);
        var completion = await service.CompleteQuestAsync(1, quest.Id);

        Assert.Equal(new[] { 4 }, completion.NewLocationIds);
        Assert.Contains(4, Assert.Single(store.ScenarioProgresses).UnlockedLocationIds);
    }

    [Fact]
    public async Task CompleteQuest_DuplicateLocationUnlocks_ReturnsLocationOnce()
    {
        var (service, store, _, _, _) = CreateScenario();
        var quest = AddSyntheticQuest(store, 511, newLocationIds: new[] { 4, 4 });

        await service.StartQuestAsync(1, quest.Id);
        var completion = await service.CompleteQuestAsync(1, quest.Id);

        Assert.Equal(new[] { 4 }, completion.NewLocationIds);
    }

    [Fact]
    public async Task CompleteQuest_AlreadyUnlockedLocation_IsNotReturnedAgain()
    {
        var (service, store, _, _, _) = CreateScenario();
        var quest = AddSyntheticQuest(store, 512, newLocationIds: new[] { 4 });
        store.ScenarioProgresses.Add(new ScenarioProgress { GameSessionId = 1, UnlockedLocationIds = new HashSet<int> { 4 } });

        await service.StartQuestAsync(1, quest.Id);
        var completion = await service.CompleteQuestAsync(1, quest.Id);

        Assert.Empty(completion.NewLocationIds);
        Assert.Contains(4, Assert.Single(store.ScenarioProgresses).UnlockedLocationIds);
    }

    [Fact]
    public async Task CompleteQuest_MixedOldAndNewLocationUnlocks_ReturnsOnlyNewLocations()
    {
        var (service, store, _, _, _) = CreateScenario();
        var quest = AddSyntheticQuest(store, 513, newLocationIds: new[] { 4, 7 });
        store.ScenarioProgresses.Add(new ScenarioProgress { GameSessionId = 1, UnlockedLocationIds = new HashSet<int> { 4 } });

        await service.StartQuestAsync(1, quest.Id);
        var completion = await service.CompleteQuestAsync(1, quest.Id);

        Assert.Equal(new[] { 7 }, completion.NewLocationIds);
        Assert.Contains(4, Assert.Single(store.ScenarioProgresses).UnlockedLocationIds);
        Assert.Contains(7, Assert.Single(store.ScenarioProgresses).UnlockedLocationIds);
    }

    // --- Objectives ---

    [Fact]
    public async Task UpdateObjective_AdvancesPastTheCompletedObjective()
    {
        var (service, store, _, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");
        await service.StartQuestAsync(1, firstMainQuestId);

        var result = await service.UpdateObjectiveAsync(1, firstMainQuestId, objectiveId: 101);

        Assert.True(result.ObjectiveCompleted);
        Assert.Equal(1, result.NextObjectiveIndex);
        Assert.Null(result.Completed);
        Assert.Empty(result.NewLocationIds);

        var active = await service.GetActiveQuestsAsync(1);
        Assert.Equal(102, Assert.Single(active).CurrentObjectiveId);
    }

    [Fact]
    public async Task UpdateObjective_ObjectiveRewardLocationUnlock_PersistsAndReturnsIt()
    {
        var (service, store, _, _, _) = CreateScenario();
        var quest = AddSyntheticQuest(store, 514);
        AddObjective(
            quest,
            objectiveId: 51401,
            newLocationIds: new[] { 4, 4 });
        AddObjective(quest, objectiveId: 51402);

        await service.StartQuestAsync(1, quest.Id);
        var result = await service.UpdateObjectiveAsync(1, quest.Id, 51401);

        Assert.Equal(new[] { 4 }, result.NewLocationIds);
        Assert.Contains(4, Assert.Single(store.ScenarioProgresses).UnlockedLocationIds);
        Assert.Null(result.Completed);
    }

    [Fact]
    public async Task UpdateObjective_ObjectiveRewardAlreadyUnlockedLocation_IsNotReturnedAgain()
    {
        var (service, store, _, _, _) = CreateScenario();
        var quest = AddSyntheticQuest(store, 515);
        AddObjective(quest, objectiveId: 51501, newLocationIds: new[] { 4 });
        AddObjective(quest, objectiveId: 51502);
        store.ScenarioProgresses.Add(new ScenarioProgress { GameSessionId = 1, UnlockedLocationIds = new HashSet<int> { 4 } });

        await service.StartQuestAsync(1, quest.Id);
        var result = await service.UpdateObjectiveAsync(1, quest.Id, 51501);

        Assert.Empty(result.NewLocationIds);
        Assert.Contains(4, Assert.Single(store.ScenarioProgresses).UnlockedLocationIds);
    }

    [Fact]
    public async Task UpdateObjective_NonCurrentObjective_IsRejected()
    {
        var (service, store, _, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");
        await service.StartQuestAsync(1, firstMainQuestId);

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.UpdateObjectiveAsync(1, firstMainQuestId, objectiveId: 103));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateObjective_AmountMustBePositive()
    {
        var (service, store, _, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");
        await service.StartQuestAsync(1, firstMainQuestId);

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.UpdateObjectiveAsync(1, firstMainQuestId, objectiveId: 101, amount: 0));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateObjective_CompletingTheLastObjective_CompletesTheQuest()
    {
        var (service, store, player, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");
        await service.StartQuestAsync(1, firstMainQuestId);

        await service.UpdateObjectiveAsync(1, firstMainQuestId, 101);
        await service.UpdateObjectiveAsync(1, firstMainQuestId, 102);
        var final = await service.UpdateObjectiveAsync(1, firstMainQuestId, 103);

        Assert.True(final.ObjectiveCompleted);
        Assert.Null(final.NextObjectiveIndex);
        Assert.NotNull(final.Completed);
        Assert.Equal(100, final.Completed.ExperienceGained);
        Assert.Equal(100, player.CurrentXp);

        var active = await service.GetActiveQuestsAsync(1);
        Assert.DoesNotContain(active, quest => quest.QuestId == firstMainQuestId);
    }

    [Fact]
    public async Task UpdateObjective_OnACompletedQuest_IsRejected()
    {
        var (service, store, _, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");
        await service.StartQuestAsync(1, firstMainQuestId);
        await service.CompleteQuestAsync(1, firstMainQuestId);

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.UpdateObjectiveAsync(1, firstMainQuestId, objectiveId: 101));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    // --- Failing quests ---

    [Fact]
    public async Task FailQuest_BlocksRestartAndCompletion()
    {
        var (service, store, player, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");
        await service.StartQuestAsync(1, firstMainQuestId);

        await service.FailQuestAsync(1, firstMainQuestId);

        await Assert.ThrowsAsync<DomainException>(() => service.CompleteQuestAsync(1, firstMainQuestId));
        await Assert.ThrowsAsync<DomainException>(() => service.StartQuestAsync(1, firstMainQuestId));
        Assert.Equal(0, player.CurrentXp);

        var available = await service.GetAvailableQuestsAsync(1);
        Assert.DoesNotContain(available, quest => quest.Id == firstMainQuestId);
    }

    // --- Ownership and catalog reads ---

    [Fact]
    public async Task GetAvailableQuests_ForSomeoneElsesSession_Fails()
    {
        var (_, store, _, _, _) = CreateScenario();
        store.GameSessions.Add(new GameSession { Id = 2, CharacterId = 99 });

        var service = CreateService(store);
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.GetAvailableQuestsAsync(2));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task GetQuestById_UnknownQuest_Fails()
    {
        var (service, _, _, _, _) = CreateScenario();

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.GetQuestByIdAsync(999999));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task CompleteQuest_OnAnUnstartedQuest_Fails()
    {
        var (service, store, _, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.CompleteQuestAsync(1, firstMainQuestId));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    // --- Player-based queries and completion listing ---

    [Fact]
    public async Task GetCompletedQuests_ReturnsOnlyCompletedQuests()
    {
        var (service, store, _, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");
        await service.StartQuestAsync(1, firstMainQuestId);
        await service.CompleteQuestAsync(1, firstMainQuestId);

        var completed = await service.GetCompletedQuestsAsync(1);
        var active = await service.GetActiveQuestsAsync(1);

        Assert.Contains(completed, quest => quest.QuestId == firstMainQuestId);
        Assert.DoesNotContain(active, quest => quest.QuestId == firstMainQuestId);
    }

    [Fact]
    public async Task GetAvailableQuestsForPlayer_UsesThePlayersSession()
    {
        var (service, store, _, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");

        var available = await service.GetAvailableQuestsForPlayerAsync(1, locationId: 3);

        Assert.Contains(available, quest => quest.Id == firstMainQuestId);
    }

    [Fact]
    public async Task StartQuestForPlayer_ActivatesTheQuest()
    {
        var (service, store, _, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");

        var started = await service.StartQuestForPlayerAsync(1, firstMainQuestId);

        Assert.Equal(QuestStatus.Active, started.Status);
    }

    [Fact]
    public async Task CompleteQuestForPlayer_GrantsExperience()
    {
        var (service, store, player, _, _) = CreateScenario();
        var firstMainQuestId = QuestId(store, "MQ-01");
        await service.StartQuestForPlayerAsync(1, firstMainQuestId);

        var completion = await service.CompleteQuestForPlayerAsync(1, firstMainQuestId);

        Assert.Equal(100, completion.ExperienceGained);
        Assert.Equal(100, player.CurrentXp);
    }

    [Fact]
    public async Task PlayerBasedQueries_UnknownPlayer_FailsWithNotFound()
    {
        var (service, _, _, _, _) = CreateScenario();

        await Assert.ThrowsAsync<DomainException>(() => service.GetActiveQuestsForPlayerAsync(99));
    }

    // --- Scenario helpers ---

    private static (IQuestService Service, InMemoryGameDataStore Store, PlayerCharacter Player, GameSession Session, LevelProgressionRules Rules) CreateScenario(
        int characterLevel = 1,
        int xp = 0)
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var player = new PlayerCharacter
        {
            Id = CurrentPlayerId,
            OwnerId = CurrentPlayerId.ToString(),
            Name = "Hero",
            Level = characterLevel,
            CurrentXp = xp,
            SkillPoints = 0,
            MaxHealth = 20,
            CurrentHealth = 20,
            RaceId = 1,
            ClassId = 1
        };
        store.Characters.Add(player);
        store.GameSessions.Add(new GameSession { Id = 1, CharacterId = CurrentPlayerId, AdventureId = 1 });

        return (CreateService(store), store, player, store.GameSessions.Single(s => s.Id == 1), new LevelProgressionRules());
    }

    private static IQuestService CreateService(InMemoryGameDataStore store)
    {
        return new QuestService(
            new MockQuestRepository(store),
            new MockPlayerQuestRepository(store),
            new MockScenarioProgressRepository(store),
            new MockGameSessionRepository(store),
            new MockCharacterRepository(store),
            new ExperienceService(new LevelProgressionRules()),
            new FixedCurrentPlayerService(CurrentPlayerId),
            new MockLocationProgressRepository(store),
            new LocationUnlockEngine(),
            new LocationRouteProvider());
    }

    private static int QuestId(InMemoryGameDataStore store, string code)
    {
        return store.Quests.Single(quest => quest.Code == code).Id;
    }

    private static Quest AddSyntheticQuest(
        InMemoryGameDataStore store,
        int id,
        string title = "Synthetic Quest",
        int recommendedLevel = 1,
        int? recommendedMaximumLevel = null,
        int? locationId = null,
        int experience = 0,
        Dictionary<string, bool>? requiredFlags = null,
        Dictionary<string, bool>? resultFlags = null,
        IReadOnlyCollection<int>? newLocationIds = null)
    {
        var quest = new Quest
        {
            Id = id,
            Code = $"SYN-{id}",
            Title = title,
            Description = "Test quest",
            QuestType = QuestType.Side,
            QuestGiver = "Quest Giver",
            RecommendedLevel = recommendedLevel,
            RecommendedMaximumLevel = recommendedMaximumLevel,
            LocationId = locationId,
            Rewards = new List<QuestReward>
            {
                new()
                {
                    Experience = experience,
                    NewLocationIds = newLocationIds?.ToList() ?? new List<int>()
                }
            },
            RequiredFlags = requiredFlags ?? new(),
            ResultFlags = resultFlags ?? new(),
            Objectives = new List<QuestObjective>()
        };
        store.Quests.Add(quest);
        return quest;
    }

    private static void AddObjective(
        Quest quest,
        int objectiveId,
        IReadOnlyCollection<int>? newLocationIds = null)
    {
        quest.Objectives.Add(new QuestObjective
        {
            Id = objectiveId,
            Description = "Synthetic objective",
            ObjectiveType = ObjectiveType.MakeChoice,
            RequiredAmount = 1,
            Rewards = new List<QuestReward>
            {
                new()
                {
                    NewLocationIds = newLocationIds?.ToList() ?? new List<int>()
                }
            }
        });
    }

    private sealed class FixedCurrentPlayerService : ICurrentPlayerService
    {
        private readonly int _playerId;

        public FixedCurrentPlayerService(int playerId)
        {
            _playerId = playerId;
        }

        public int GetCurrentPlayerId() => _playerId;
    }
}
