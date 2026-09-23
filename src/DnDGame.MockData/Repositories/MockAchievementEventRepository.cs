using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Achievements;

namespace DnDGame.MockData.Repositories;

public class MockAchievementEventRepository : IAchievementEventRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockAchievementEventRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<bool> AddIfMissingAsync(AchievementEvent recordedEvent)
    {
        var alreadyCounted = _store.AchievementEvents.Any(candidate =>
            candidate.PlayerId == recordedEvent.PlayerId
            && candidate.Type == recordedEvent.Type
            && string.Equals(candidate.EventKey, recordedEvent.EventKey, StringComparison.Ordinal));

        if (alreadyCounted)
        {
            return Task.FromResult(false);
        }

        _store.AchievementEvents.Add(recordedEvent);
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<AchievementEvent>> GetByPlayerIdAsync(int playerId)
    {
        var rows = _store.AchievementEvents.Where(@event => @event.PlayerId == playerId).ToList();
        return Task.FromResult<IReadOnlyList<AchievementEvent>>(rows);
    }
}