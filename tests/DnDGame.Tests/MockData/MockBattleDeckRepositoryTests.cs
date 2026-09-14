using DnDGame.Domain.Entities.Game;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;

namespace DnDGame.Tests.MockData;

public class MockBattleDeckRepositoryTests
{
    [Fact]
    public async Task AddAsync_AssignsIncrementingIds()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockBattleDeckRepository(store);

        var first = await repository.AddAsync(new BattleDeck { BattleId = 1, DeckId = 1 });
        var second = await repository.AddAsync(new BattleDeck { BattleId = 2, DeckId = 1 });

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMatchingBattleDeck()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockBattleDeckRepository(store);
        var battleDeck = await repository.AddAsync(new BattleDeck { BattleId = 1, DeckId = 1 });

        var found = await repository.GetByIdAsync(battleDeck.Id);

        Assert.NotNull(found);
        Assert.Equal(battleDeck.Id, found!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockBattleDeckRepository(store);

        var found = await repository.GetByIdAsync(999);

        Assert.Null(found);
    }

    [Fact]
    public async Task GetByBattleIdAsync_ReturnsBattleDeckForThatBattle()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockBattleDeckRepository(store);
        await repository.AddAsync(new BattleDeck { BattleId = 1, DeckId = 1 });
        var forBattleTwo = await repository.AddAsync(new BattleDeck { BattleId = 2, DeckId = 1 });

        var found = await repository.GetByBattleIdAsync(2);

        Assert.NotNull(found);
        Assert.Equal(forBattleTwo.Id, found!.Id);
    }

    [Fact]
    public async Task GetByBattleIdAsync_UnknownBattleId_ReturnsNull()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockBattleDeckRepository(store);

        var found = await repository.GetByBattleIdAsync(999);

        Assert.Null(found);
    }
}
