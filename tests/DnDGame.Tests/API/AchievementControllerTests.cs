using DnDGame.API.Controllers;
using DnDGame.BusinessLayer.Dtos.Achievements;
using DnDGame.BusinessLayer.Services;
using DnDGame.Domain.Entities.Characters;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using DnDGame.MockData.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DnDGame.Tests.API;

/// <summary>
/// API-surface tests for the achievements endpoints. The player identity comes from
/// MockCurrentPlayerService (same as the rest of the API); the store is the seeded
/// singleton backing both the "make progress" and the "resume" service instances.
/// </summary>
public class AchievementControllerTests
{
    [Fact]
    public async Task GetOverview_ReturnsTheCatalogGroupedIntoStateBuckets()
    {
        var (service, store) = CreateServiceWithCharacter();

        await service.RegisterCharacterCreatedAsync(1);
        await service.RegisterQuestCompletedAsync(1, questId: 1);

        var controller = new AchievementController(service);

        var response = await controller.GetOverview();

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var overview = Assert.IsType<AchievementsOverviewDto>(result.Value);
        Assert.Equal(1, overview.PlayerId);
        Assert.Equal(7, overview.TotalCount);
        Assert.Equal(2, overview.CompletedCount);
        Assert.Equal(4, overview.Locked.Count);
        Assert.Single(overview.InProgress);
        Assert.Equal(2, overview.Unlocked.Count);

        // Buckets partition the full catalog: no duplicates, nothing dropped.
        var allIds = overview.Locked.Select(entry => entry.Id)
            .Concat(overview.InProgress.Select(entry => entry.Id))
            .Concat(overview.Unlocked.Select(entry => entry.Id))
            .ToList();
        Assert.Equal(overview.TotalCount, allIds.Distinct().Count());
        Assert.Equal(1, overview.Unlocked.Single(entry => entry.Code == "A_HERO_IS_BORN").CurrentAmount);
        Assert.Equal(1, overview.InProgress.Single(entry => entry.Code == "QUEST_CONQUEROR").CurrentAmount);
    }

    private static (AchievementService Service, InMemoryGameDataStore Store) CreateServiceWithCharacter()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var currentPlayer = new MockCurrentPlayerService();
        store.Characters.Add(new PlayerCharacter
        {
            Id = 1,
            OwnerId = currentPlayer.GetCurrentPlayerId().ToString(),
            Name = "Hero",
            Level = 1,
            RaceId = 1,
            ClassId = 1
        });

        var service = new AchievementService(
            new MockAchievementRepository(store),
            new MockAchievementProgressRepository(store),
            new MockAchievementEventRepository(store),
            new MockCharacterRepository(store),
            currentPlayer);

        return (service, store);
    }
}