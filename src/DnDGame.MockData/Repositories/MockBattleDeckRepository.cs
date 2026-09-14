using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.MockData.Repositories;

public class MockBattleDeckRepository : IBattleDeckRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockBattleDeckRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<BattleDeck?> GetByIdAsync(int id)
    {
        var battleDeck = _store.BattleDecks.FirstOrDefault(bd => bd.Id == id);
        return Task.FromResult(battleDeck);
    }

    public Task<BattleDeck?> GetByDeckIdAsync(int deckId)
    {
        var battleDeck = _store.BattleDecks.FirstOrDefault(bd => bd.DeckId == deckId);
        return Task.FromResult(battleDeck);
    }

    public Task<BattleDeck> AddAsync(BattleDeck battleDeck)
    {
        battleDeck.Id = _store.GetNextBattleDeckId();
        _store.BattleDecks.Add(battleDeck);
        return Task.FromResult(battleDeck);
    }

    public Task UpdateAsync(BattleDeck battleDeck)
    {
        return Task.CompletedTask;
    }
}
