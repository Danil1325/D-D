using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Enemies;

namespace DnDGame.MockData.Repositories;

public class MockEnemyRepository : IEnemyRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockEnemyRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Enemy>> GetAllAsync()
    {
        IReadOnlyList<Enemy> enemies = _store.Enemies.ToList();
        return Task.FromResult(enemies);
    }

    public Task<Enemy?> GetByIdAsync(int id)
    {
        var enemy = _store.Enemies.FirstOrDefault(e => e.Id == id);
        return Task.FromResult(enemy);
    }
}
