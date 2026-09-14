using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;

namespace DnDGame.Tests.MockData;

public class MockBattleRepositoryTests
{
    [Fact]
    public async Task AddAsync_AssignsIncrementingIds()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockBattleRepository(store);

        var first = await repository.AddAsync(new Battle { GameSessionId = 1 });
        var second = await repository.AddAsync(new Battle { GameSessionId = 1 });

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMatchingBattle()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockBattleRepository(store);
        var battle = await repository.AddAsync(new Battle { GameSessionId = 1 });

        var found = await repository.GetByIdAsync(battle.Id);

        Assert.NotNull(found);
        Assert.Equal(battle.Id, found!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockBattleRepository(store);

        var found = await repository.GetByIdAsync(999);

        Assert.Null(found);
    }

    [Fact]
    public async Task GetActiveByGameSessionIdAsync_OnlyReturnsInProgressBattleForThatSession()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockBattleRepository(store);
        await repository.AddAsync(new Battle { GameSessionId = 1, Status = GameSessionStatus.Victory });
        var active = await repository.AddAsync(new Battle { GameSessionId = 1, Status = GameSessionStatus.InProgress });
        await repository.AddAsync(new Battle { GameSessionId = 2, Status = GameSessionStatus.InProgress });

        var found = await repository.GetActiveByGameSessionIdAsync(1);

        Assert.NotNull(found);
        Assert.Equal(active.Id, found!.Id);
    }

    [Fact]
    public async Task GetActiveByGameSessionIdAsync_NoInProgressBattle_ReturnsNull()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockBattleRepository(store);
        await repository.AddAsync(new Battle { GameSessionId = 1, Status = GameSessionStatus.Defeat });

        var found = await repository.GetActiveByGameSessionIdAsync(1);

        Assert.Null(found);
    }
}
