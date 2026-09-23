using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Achievements;

namespace DnDGame.MockData.Repositories;

public class MockAchievementProgressRepository : IAchievementProgressRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockAchievementProgressRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<AchievementProgress>> GetByPlayerIdAsync(int playerId)
    {
        var rows = _store.AchievementProgresses.Where(p => p.PlayerId == playerId).ToList();
        return Task.FromResult<IReadOnlyList<AchievementProgress>>(rows);
    }

    public Task<AchievementProgress?> GetAsync(int playerId, int achievementId)
    {
        var row = _store.AchievementProgresses.FirstOrDefault(p => p.PlayerId == playerId && p.AchievementId == achievementId);
        return Task.FromResult(row);
    }

    public Task<AchievementProgress> AddAsync(AchievementProgress progress)
    {
        _store.AchievementProgresses.Add(progress);
        return Task.FromResult(progress);
    }

    public Task UpdateAsync(AchievementProgress progress)
    {
        return Task.CompletedTask;
    }
}