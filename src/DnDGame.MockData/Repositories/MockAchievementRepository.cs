using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Achievements;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.Repositories;

public class MockAchievementRepository : IAchievementRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockAchievementRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Achievement>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<Achievement>>(_store.Achievements.ToList());
    }

    public Task<IReadOnlyList<Achievement>> GetByTypeAsync(AchievementType type)
    {
        var rows = _store.Achievements.Where(a => a.Type == type).ToList();
        return Task.FromResult<IReadOnlyList<Achievement>>(rows);
    }
}