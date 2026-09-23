using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Achievements;
using DnDGame.BusinessLayer.Dtos.Characters;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using DnDGame.MockData.Services;

namespace DnDGame.Tests.BusinessLayer;

public class AchievementServiceTests
{
    private const int PlayerCharacterId = 1;

    // --- Catalog ---

    [Fact]
    public async Task GetCatalogAsync_ReturnsTheSeededAchievements()
    {
        var (service, _) = CreateService();

        var catalog = await service.GetCatalogAsync();

        Assert.Equal(7, catalog.Count);
        Assert.Contains(catalog, entry => entry.Code == "A_HERO_IS_BORN");
        Assert.All(catalog, entry => Assert.True(entry.TargetAmount > 0));
    }

    // --- Progress ---

    [Fact]
    public async Task RegisterCharacterCreated_CompletesTheCreationAchievement()
    {
        var (service, _) = CreateService();

        await service.RegisterCharacterCreatedAsync(PlayerCharacterId);

        var progress = await service.GetProgressForPlayerAsync(PlayerCharacterId);
        var creation = Assert.Single(progress.Achievements, entry => entry.Code == "A_HERO_IS_BORN");
        Assert.Equal(1, creation.CurrentAmount);
        Assert.True(creation.IsCompleted);
        Assert.NotNull(creation.CompletedAt);
        Assert.Equal(1, progress.CompletedCount);
        Assert.Equal(7, progress.TotalCount);
    }

    [Fact]
    public async Task EventsForOnePlayer_DoNotCountTowardAnother()
    {
        var (service, _) = CreateService();

        await service.RegisterQuestCompletedAsync(PlayerCharacterId, questId: 1);

        var other = await service.GetProgressForPlayerAsync(2);
        Assert.Equal(0, other.Achievements.Single(entry => entry.Code == "FIRST_STEPS").CurrentAmount);
    }

    [Fact]
    public async Task GetProgressForCurrentPlayer_ResolvesTheCharacterByOwnerId()
    {
        var (service, store) = CreateService();
        store.Characters.Add(new PlayerCharacter
        {
            Id = PlayerCharacterId,
            OwnerId = MockCurrentPlayerService.MockPlayerId.ToString(),
            Name = "Hero",
            Level = 1,
            RaceId = 1,
            ClassId = 1
        });

        var progress = await service.GetProgressForCurrentPlayerAsync();

        Assert.Equal(PlayerCharacterId, progress.PlayerId);
        Assert.Equal(7, progress.TotalCount);
    }

    [Fact]
    public async Task GetProgressForCurrentPlayer_WhenNoCharacter_ThrowsNotFound()
    {
        var (service, _) = CreateService();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetProgressForCurrentPlayerAsync());

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    // --- Overview (the achievements screen after the game is resumed) ---

    [Fact]
    public async Task GetOverviewForCurrentPlayer_WhenNoCharacter_ThrowsNotFound()
    {
        var (service, _) = CreateService();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetOverviewForCurrentPlayerAsync());

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task GetOverviewForCurrentPlayer_GroupsEveryAchievementIntoItsStateBucket()
    {
        var (service, store) = CreateService();
        store.Characters.Add(new PlayerCharacter
        {
            Id = PlayerCharacterId,
            OwnerId = MockCurrentPlayerService.MockPlayerId.ToString(),
            Name = "Hero",
            Level = 1,
            RaceId = 1,
            ClassId = 1
        });

        await service.RegisterCharacterCreatedAsync(PlayerCharacterId);
        await service.RegisterQuestCompletedAsync(PlayerCharacterId, questId: 1);
        await service.RegisterBattleVictoryAsync(PlayerCharacterId, battleId: 1);

        var overview = await service.GetOverviewForCurrentPlayerAsync();

        Assert.Equal(PlayerCharacterId, overview.PlayerId);
        Assert.Equal(7, overview.TotalCount);
        Assert.Equal(3, overview.CompletedCount);

        // A_HERO_IS_BORN, FIRST_STEPS and FIRST_BLOOD reached their target.
        Assert.Equal(new[] { "A_HERO_IS_BORN", "FIRST_STEPS", "FIRST_BLOOD" },
            overview.Unlocked.Select(entry => entry.Code));
        Assert.All(overview.Unlocked, entry => Assert.NotNull(entry.CompletedAt));
        Assert.All(overview.Unlocked, entry => Assert.True(entry.IsCompleted));

        // QUEST_CONQUEROR (1/5) and VICTORIOUS_WARRIOR (1/10) have progress but are not done.
        Assert.Equal(new[] { "QUEST_CONQUEROR", "VICTORIOUS_WARRIOR" },
            overview.InProgress.Select(entry => entry.Code));
        Assert.All(overview.InProgress, entry => Assert.False(entry.IsCompleted));
        Assert.All(overview.InProgress, entry => Assert.Null(entry.CompletedAt));

        // No location has been unlocked, so WANDERER and EXPLORER are still locked.
        Assert.Equal(new[] { "WANDERER", "EXPLORER" }, overview.Locked.Select(entry => entry.Code));
        Assert.All(overview.Locked, entry => Assert.Equal(0, entry.CurrentAmount));

        // Every catalog entry appears in exactly one bucket.
        Assert.Equal(7, overview.Locked.Count + overview.InProgress.Count + overview.Unlocked.Count);
    }

    [Fact]
    public async Task GetOverviewForCurrentPlayer_AfterResume_ReturnsTheSameProgress()
    {
        var (service, store) = CreateService();
        store.Characters.Add(new PlayerCharacter
        {
            Id = PlayerCharacterId,
            OwnerId = MockCurrentPlayerService.MockPlayerId.ToString(),
            Name = "Hero",
            Level = 1,
            RaceId = 1,
            ClassId = 1
        });

        await service.RegisterCharacterCreatedAsync(PlayerCharacterId);
        await service.RegisterQuestCompletedAsync(PlayerCharacterId, questId: 1);
        await service.RegisterQuestCompletedAsync(PlayerCharacterId, questId: 2);
        var baseline = await service.GetOverviewForCurrentPlayerAsync();

        // A page reload after resuming the game resolves a fresh scoped service
        // over the same singleton store — exactly what the API does on the next
        // GET /api/character/current + GET /api/achievements/overview.
        var resumedService = CreateAchievementService(store, new MockCurrentPlayerService());
        var afterResume = await resumedService.GetOverviewForCurrentPlayerAsync();

        Assert.Equal(baseline.PlayerId, afterResume.PlayerId);
        Assert.Equal(baseline.TotalCount, afterResume.TotalCount);
        Assert.Equal(baseline.CompletedCount, afterResume.CompletedCount);
        Assert.Equal(baseline.Locked.Select(entry => entry.Code), afterResume.Locked.Select(entry => entry.Code));
        Assert.Equal(baseline.InProgress.Select(entry => entry.Code), afterResume.InProgress.Select(entry => entry.Code));
        Assert.Equal(baseline.Unlocked.Select(entry => entry.Code), afterResume.Unlocked.Select(entry => entry.Code));
        Assert.Equal(baseline.Unlocked.Select(entry => entry.CurrentAmount), afterResume.Unlocked.Select(entry => entry.CurrentAmount));
        Assert.Equal(baseline.Unlocked.Select(entry => entry.CompletedAt), afterResume.Unlocked.Select(entry => entry.CompletedAt));
        Assert.Equal(baseline.InProgress.Select(entry => entry.CurrentAmount), afterResume.InProgress.Select(entry => entry.CurrentAmount));
    }

    // --- Thresholds ---

    [Fact]
    public async Task ThresholdAchievement_CompletesAfterDistinctEventsReachTarget()
    {
        var (service, _) = CreateService();

        for (var questId = 1; questId <= 5; questId++)
        {
            await service.RegisterQuestCompletedAsync(PlayerCharacterId, questId);
        }

        var conqueror = (await service.GetProgressForPlayerAsync(PlayerCharacterId))
            .Achievements.Single(entry => entry.Code == "QUEST_CONQUEROR");
        Assert.Equal(5, conqueror.CurrentAmount);
        Assert.True(conqueror.IsCompleted);
        Assert.NotNull(conqueror.CompletedAt);
    }

    [Fact]
    public async Task ThresholdAchievement_DoesNotCompleteBelowTarget()
    {
        var (service, _) = CreateService();

        for (var battleId = 1; battleId <= 9; battleId++)
        {
            await service.RegisterBattleVictoryAsync(PlayerCharacterId, battleId);
        }

        var warrior = (await service.GetProgressForPlayerAsync(PlayerCharacterId))
            .Achievements.Single(entry => entry.Code == "VICTORIOUS_WARRIOR");
        Assert.Equal(9, warrior.CurrentAmount);
        Assert.False(warrior.IsCompleted);
        Assert.Null(warrior.CompletedAt);
    }

    // --- Repeated events are idempotent ---

    [Fact]
    public async Task SameQuestEvent_ReportedTwice_CountsOnce()
    {
        var (service, store) = CreateService();

        await service.RegisterQuestCompletedAsync(PlayerCharacterId, questId: 7);
        await service.RegisterQuestCompletedAsync(PlayerCharacterId, questId: 7);

        var firstStep = (await service.GetProgressForPlayerAsync(PlayerCharacterId))
            .Achievements.Single(entry => entry.Code == "FIRST_STEPS");
        Assert.Equal(1, firstStep.CurrentAmount);
        Assert.True(firstStep.IsCompleted);

        var conqueror = (await service.GetProgressForPlayerAsync(PlayerCharacterId))
            .Achievements.Single(entry => entry.Code == "QUEST_CONQUEROR");
        Assert.Equal(1, conqueror.CurrentAmount);
        Assert.False(conqueror.IsCompleted);

        Assert.Single(store.AchievementEvents, @event => @event.Type == AchievementType.QuestsCompleted);
    }

    [Fact]
    public async Task SameBattleEvent_ReportedTwice_DoesNotInflateTheThreshold()
    {
        var (service, _) = CreateService();

        await service.RegisterBattleVictoryAsync(PlayerCharacterId, battleId: 3);
        await service.RegisterBattleVictoryAsync(PlayerCharacterId, battleId: 3);
        await service.RegisterBattleVictoryAsync(PlayerCharacterId, battleId: 3);

        var warrior = (await service.GetProgressForPlayerAsync(PlayerCharacterId))
            .Achievements.Single(entry => entry.Code == "VICTORIOUS_WARRIOR");
        Assert.Equal(1, warrior.CurrentAmount);
    }

    [Fact]
    public async Task SameLocationEvent_ReportedTwice_CountsOnce()
    {
        var (service, store) = CreateService();

        await service.RegisterLocationUnlockedAsync(PlayerCharacterId, DnDGame.Domain.Entities.Locations.LocationId.HerosOverlook);
        await service.RegisterLocationUnlockedAsync(PlayerCharacterId, DnDGame.Domain.Entities.Locations.LocationId.HerosOverlook);

        var wanderer = (await service.GetProgressForPlayerAsync(PlayerCharacterId))
            .Achievements.Single(entry => entry.Code == "WANDERER");
        Assert.Equal(1, wanderer.CurrentAmount);
        Assert.Single(store.AchievementEvents, @event => @event.Type == AchievementType.LocationsUnlocked);
    }

    // --- The unlock timestamp is preserved ---

    [Fact]
    public async Task CompletedAt_IsTheFirstTimeTheThresholdWasReachedAndNeverOverwritten()
    {
        var (service, _) = CreateService();

        await service.RegisterQuestCompletedAsync(PlayerCharacterId, questId: 1);
        var completedAt = (await service.GetProgressForPlayerAsync(PlayerCharacterId))
            .Achievements.Single(entry => entry.Code == "FIRST_STEPS").CompletedAt;
        Assert.NotNull(completedAt);

        // A repeated report of the same (already counted) event must not touch it.
        await service.RegisterQuestCompletedAsync(PlayerCharacterId, questId: 1);

        var again = (await service.GetProgressForPlayerAsync(PlayerCharacterId))
            .Achievements.Single(entry => entry.Code == "FIRST_STEPS");
        Assert.Equal(completedAt, again.CompletedAt);
        Assert.Equal(1, again.CurrentAmount);
    }

    // --- Hook wiring: events fired by the application services land in the store ---

    [Fact]
    public async Task CharacterCreation_AdvancesTheCreationAchievement()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var currentPlayer = new MockCurrentPlayerService();
        var characterService = new CharacterService(
            new MockCharacterRepository(store),
            new MockRaceRepository(store),
            new MockClassRepository(store),
            new MockCharacterPortraitRepository(store),
            currentPlayer,
            CreateAchievementService(store, currentPlayer));

        await characterService.CreateNewGameAsync(new NewGameCharacterRequestDto
        {
            Name = "Hero",
            Race = RaceType.Human,
            ClassId = 1
        });

        var creation = Assert.Single(store.AchievementProgresses, row => row.AchievementId == 101);
        Assert.Equal(1, creation.CurrentAmount);
        Assert.NotNull(creation.CompletedAt);
        var eventLeadgerRow = Assert.Single(store.AchievementEvents, @event => @event.Type == AchievementType.CharacterCreated);
        Assert.Equal(PlayerCharacterId.ToString(), eventLeadgerRow.EventKey);
    }

    [Fact]
    public async Task ExplicitLocationUnlock_AdvancesTheLocationAchievement()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var currentPlayer = new MockCurrentPlayerService();
        var character = new PlayerCharacter
        {
            Id = PlayerCharacterId,
            OwnerId = currentPlayer.GetCurrentPlayerId().ToString(),
            Name = "Hero",
            Level = 1,
            RaceId = 1,
            ClassId = 1
        };
        store.Characters.Add(character);

        var unlockService = new ExplicitLocationUnlockService(
            new MockLocationDefinitionRepository(store),
            new MockLocationProgressRepository(store),
            CreateAchievementService(store, currentPlayer));

        var unlocked = await unlockService.UnlockExplicitLocationsAsync(character, new[] { 4 });

        Assert.NotEmpty(unlocked);
        var wanderer = Assert.Single(store.AchievementProgresses, row => row.AchievementId == 401);
        Assert.Equal(1, wanderer.CurrentAmount);
        Assert.NotNull(wanderer.CompletedAt);
    }

    private static (AchievementService Service, InMemoryGameDataStore Store) CreateService()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var service = CreateAchievementService(store, new MockCurrentPlayerService());
        return (service, store);
    }

    private static AchievementService CreateAchievementService(InMemoryGameDataStore store, ICurrentPlayerService currentPlayerService)
    {
        return new AchievementService(
            new MockAchievementRepository(store),
            new MockAchievementProgressRepository(store),
            new MockAchievementEventRepository(store),
            new MockCharacterRepository(store),
            currentPlayerService);
    }
}