using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.Repositories;

public class MockBattleRepository : IBattleRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockBattleRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<Battle?> GetByIdAsync(int id)
    {
        var battle = _store.Battles.FirstOrDefault(b => b.Id == id);
        return Task.FromResult(battle);
    }

    public Task<Battle?> GetActiveByGameSessionIdAsync(int gameSessionId)
    {
        var battle = _store.Battles.FirstOrDefault(b =>
            b.GameSessionId == gameSessionId && b.Status == GameSessionStatus.InProgress);
        return Task.FromResult(battle);
    }

    public Task<Battle> AddAsync(Battle battle)
    {
        battle.Id = _store.GetNextBattleId();
        _store.Battles.Add(battle);
        return Task.FromResult(battle);
    }

    public Task UpdateAsync(Battle battle)
    {
        return Task.CompletedTask;
    }
}
