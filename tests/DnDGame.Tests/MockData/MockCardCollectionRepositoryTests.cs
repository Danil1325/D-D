using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;

namespace DnDGame.Tests.MockData;

public class MockCardCollectionRepositoryTests
{
    [Fact]
    public async Task GetOrCreateByPlayerCharacterIdAsync_NoExistingCollection_CreatesAndStoresAnEmptyOne()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockCardCollectionRepository(store);

        var collection = await repository.GetOrCreateByPlayerCharacterIdAsync(1);

        Assert.Equal(1, collection.PlayerCharacterId);
        Assert.Empty(collection.Cards);
        Assert.Single(store.CardCollections);
    }

    [Fact]
    public async Task GetOrCreateByPlayerCharacterIdAsync_CalledTwice_ReturnsTheSameStoredCollection()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockCardCollectionRepository(store);

        var first = await repository.GetOrCreateByPlayerCharacterIdAsync(1);
        first.AddLockedCard(1, CardVisibilityRule.ShowNameOnly);
        var second = await repository.GetOrCreateByPlayerCharacterIdAsync(1);

        Assert.Single(store.CardCollections);
        Assert.Single(second.Cards);
    }

    [Fact]
    public async Task GetOrCreateByPlayerCharacterIdAsync_HydratesCardNavigationFromTheCatalogue()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var repository = new MockCardCollectionRepository(store);
        var catalogueCard = store.Cards.First();

        var collection = await repository.GetOrCreateByPlayerCharacterIdAsync(1);
        collection.AddLockedCard(catalogueCard.Id, CardVisibilityRule.ShowNameOnly);
        var rehydrated = await repository.GetOrCreateByPlayerCharacterIdAsync(1);

        var playerCard = Assert.Single(rehydrated.Cards);
        Assert.NotNull(playerCard.Card);
        Assert.Equal(catalogueCard.Name, playerCard.Card!.Name);
    }
}
