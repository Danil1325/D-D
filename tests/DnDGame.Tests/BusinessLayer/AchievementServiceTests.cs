using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Achievements;
using DnDGame.BusinessLayer.Dtos.Characters;
using DnDGame.BusinessLayer.Services;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using DnDGame.MockData.Services;

namespace DnDGame.Tests.BusinessLayer;

public class AchievementServiceTests
{
    private const int PlayerCharacterId = 1;

    [Fact]
    public async Task GetCatalogAsync_ReturnsTheSeededAchievements()
    {
        var (service, _) = CreateService();

        var catalog = await service.GetCatalogAsync();

        Assert.Equal(7, catalog.Count);
        Assert.Contains(catalog, entry => entry.Code == "A_HERO_IS_BORN");
        Assert.All(catalog, entry => Assert.True(entry.TargetAmount > 0));
    }

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
    }

    [Fact]
    public async Task RegisterQuestCompleted_ReachesTargetAfterEnoughQuests()
    {
        var (service, _) = CreateService();

        await service.RegisterQuestCompletedAsync(PlayerCharacterId);

        var firstStep = (await service.GetProgressForPlayerAsync(PlayerCharacterId))
            .Achievements.Single(entry => entry.Code == "FIRST_STEPS");
        Assert.True(firstStep.IsCompleted);

        await service.RegisterQuestCompletedAsync(PlayerCharacterId);

        var conqueror = (await service.GetProgressForPlayerAsync(PlayerCharacterId))
            .Achievements.Single(entry => entry.Code == "QUEST_CONQUEROR");
        Assert.Equal(2, conqueror.CurrentAmount);
        Assert.False(conqueror.IsCompleted);
    }

    [Fact]
    public async Task RegisterBattleVictory_AfterTarget_IgnoresFurtherEvents()
    {
        var (service, _) = CreateService();

        for (var i = 0; i < 10; i++)
        {
            await service.RegisterBattleVictoryAsync(PlayerCharacterId);
        }

        var before = (await service.GetProgressForPlayerAsync(PlayerCharacterId))
            .Achievements.Single(entry => entry.Code == "VICTORIOUS_WARRIOR");
        Assert.Equal(10, before.CurrentAmount);
        Assert.True(before.IsCompleted);

        await service.RegisterBattleVictoryAsync(PlayerCharacterId);

        var after = (await service.GetProgressForPlayerAsync(PlayerCharacterId))
            .Achievements.Single(entry => entry.Code == "VICTORIOUS_WARRIOR");
        Assert.Equal(10, after.CurrentAmount);
    }

    [Fact]
    public async Task EventsForOnePlayer_DoNotCountTowardAnother()
    {
        var (service, _) = CreateService();

        await service.RegisterQuestCompletedAsync(PlayerCharacterId);

        var other = await service.GetProgressForPlayerAsync(2);
        Assert.Equal(0, other.Achievements.Single(entry => entry.Code == "FIRST_STEPS").CurrentAmount);
    }

    [Fact]
    public async Task GetProgressForCurrentPlayer_ResolvesTheCharacterByOwnerId()
    {
        var (service, store) = CreateService();
        store.Characters.Add(new DnDGame.Domain.Entities.Characters.PlayerCharacter
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

    private static (AchievementService Service, InMemoryGameDataStore Store) CreateService()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var service = new AchievementService(
            new MockAchievementRepository(store),
            new MockAchievementProgressRepository(store),
            new MockCharacterRepository(store),
            new MockCurrentPlayerService());
        return (service, store);
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
            new AchievementService(
                new MockAchievementRepository(store),
                new MockAchievementProgressRepository(store),
                new MockCharacterRepository(store),
                currentPlayer));

        await characterService.CreateNewGameAsync(new NewGameCharacterRequestDto
        {
            Name = "Hero",
            Race = RaceType.Human,
            ClassId = 1
        });

        var creation = Assert.Single(store.AchievementProgresses, row => row.AchievementId == 101);
        Assert.Equal(1, creation.CurrentAmount);
        Assert.NotNull(creation.CompletedAt);
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
            new AchievementService(
                new MockAchievementRepository(store),
                new MockAchievementProgressRepository(store),
                new MockCharacterRepository(store),
                currentPlayer));

        var unlocked = await unlockService.UnlockExplicitLocationsAsync(character, new[] { 4 });

        Assert.NotEmpty(unlocked);
        var wanderer = Assert.Single(store.AchievementProgresses, row => row.AchievementId == 401);
        Assert.Equal(1, wanderer.CurrentAmount);
        Assert.NotNull(wanderer.CompletedAt);
    }
}