using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Cards;

namespace DnDGame.MockData.Repositories;

public class MockCardCollectionRepository : ICardCollectionRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockCardCollectionRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<CardCollection> GetOrCreateByPlayerCharacterIdAsync(int playerCharacterId)
    {
        var collection = _store.CardCollections.FirstOrDefault(c => c.PlayerCharacterId == playerCharacterId);
        if (collection is null)
        {
            collection = new CardCollection(playerCharacterId);
            _store.CardCollections.Add(collection);
        }

        return Task.FromResult(Hydrate(collection));
    }

    private CardCollection Hydrate(CardCollection collection)
    {
        foreach (var playerCard in collection.Cards)
        {
            playerCard.Card = _store.Cards.FirstOrDefault(c => c.Id == playerCard.CardId);
        }

        return collection;
    }
}
