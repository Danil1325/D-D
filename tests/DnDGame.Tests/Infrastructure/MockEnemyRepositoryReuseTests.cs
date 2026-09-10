using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;

namespace DnDGame.Tests.Infrastructure;

/// <summary>
/// Confirms the pre-existing MockEnemyRepository (built in an earlier phase, for
/// the adventure system) still works when reused for the card-battle system's
/// first battle test, per Task 3.8. This does not add any new enemy data — it just
/// verifies the existing seed data has a usable Goblin.
/// </summary>
public class MockEnemyRepositoryReuseTests
{
    [Fact]
    public async Task GetAllAsync_IncludesGoblin_UsableForFirstBattleTest()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var repository = new MockEnemyRepository(store);

        var enemies = await repository.GetAllAsync();
        var goblin = enemies.FirstOrDefault(e => e.Name == "Goblin");

        Assert.NotNull(goblin);
        Assert.Equal(EnemyFamily.Goblin, goblin!.Family);
        Assert.Equal(EnemyTier.Base, goblin.Tier);
        Assert.True(goblin.Health > 0);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var repository = new MockEnemyRepository(store);

        var enemy = await repository.GetByIdAsync(9999);

        Assert.Null(enemy);
    }
}
