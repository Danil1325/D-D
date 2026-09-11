using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;

namespace DnDGame.Tests.MockData;

public class MockDeckRepositoryTests
{
    [Fact]
    public async Task AddAsync_AssignsIncrementingIds()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockDeckRepository(store);

        var first = await repository.AddAsync(new Deck { Name = "Deck A", CharacterId = 1 });
        var second = await repository.AddAsync(new Deck { Name = "Deck B", CharacterId = 1 });

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }

    [Fact]
    public async Task GetAllByCharacterIdAsync_OnlyReturnsThatCharactersDecks()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockDeckRepository(store);
        await repository.AddAsync(new Deck { Name = "Mine", CharacterId = 1 });
        await repository.AddAsync(new Deck { Name = "Someone else's", CharacterId = 2 });

        var decks = await repository.GetAllByCharacterIdAsync(1);

        var deck = Assert.Single(decks);
        Assert.Equal("Mine", deck.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesDeck_ReturnsTrue()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockDeckRepository(store);
        var deck = await repository.AddAsync(new Deck { Name = "Deck A", CharacterId = 1 });

        var deleted = await repository.DeleteAsync(deck.Id);

        Assert.True(deleted);
        Assert.Null(await repository.GetByIdAsync(deck.Id));
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ReturnsFalse()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockDeckRepository(store);

        var deleted = await repository.DeleteAsync(999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task GetCardsByIdsAsync_ReturnsOnlyMatchingCatalogueCards()
    {
        var store = new InMemoryGameDataStore();
        store.Cards.Add(new Card { Id = 1, Name = "Card 1" });
        store.Cards.Add(new Card { Id = 2, Name = "Card 2" });
        var repository = new MockDeckRepository(store);

        var cards = await repository.GetCardsByIdsAsync([1, 999]);

        var card = Assert.Single(cards);
        Assert.Equal("Card 1", card.Name);
    }
}
