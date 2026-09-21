using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Locations;

namespace DnDGame.MockData.Repositories;

public class MockLocationProgressRepository : ILocationProgressRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockLocationProgressRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<LocationProgress>> GetByPlayerIdAsync(int playerId)
    {
        var rows = _store.LocationProgresses.Where(p => p.PlayerId == playerId).ToList();
        return Task.FromResult<IReadOnlyList<LocationProgress>>(rows);
    }

    public Task<LocationProgress?> GetAsync(int playerId, LocationId locationId)
    {
        var row = _store.LocationProgresses.FirstOrDefault(p => p.PlayerId == playerId && p.LocationId == locationId);
        return Task.FromResult(row);
    }

    public Task<LocationProgress> AddAsync(LocationProgress progress)
    {
        _store.LocationProgresses.Add(progress);
        return Task.FromResult(progress);
    }

    public Task UpdateAsync(LocationProgress progress)
    {
        return Task.CompletedTask;
    }
}
