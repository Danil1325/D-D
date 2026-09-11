using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.MockData.Repositories;

public class MockDeckRepository : IDeckRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockDeckRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Deck>> GetAllByCharacterIdAsync(int characterId)
    {
        IReadOnlyList<Deck> decks = _store.Decks
            .Where(deck => deck.CharacterId == characterId)
            .ToList();
        return Task.FromResult(decks);
    }

    public Task<Deck?> GetByIdAsync(int id)
    {
        var deck = _store.Decks.FirstOrDefault(d => d.Id == id);
        return Task.FromResult(deck);
    }

    public Task<Deck> AddAsync(Deck deck)
    {
        deck.Id = _store.GetNextDeckId();
        _store.Decks.Add(deck);
        return Task.FromResult(deck);
    }

    public Task UpdateAsync(Deck deck)
    {
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(int id)
    {
        var deck = _store.Decks.FirstOrDefault(d => d.Id == id);
        if (deck is null)
        {
            return Task.FromResult(false);
        }

        _store.Decks.Remove(deck);
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<Card>> GetCardsByIdsAsync(IEnumerable<int> cardIds)
    {
        var idSet = cardIds.ToHashSet();
        IReadOnlyList<Card> cards = _store.Cards.Where(c => idSet.Contains(c.Id)).ToList();
        return Task.FromResult(cards);
    }
}
