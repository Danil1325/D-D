using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.MockData.Repositories;

public class MockLocationRepository : ILocationRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockLocationRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Location>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<Location>>(_store.Locations.ToList());
    }

    public Task<Location?> GetByIdAsync(int id)
    {
        return Task.FromResult(_store.Locations.FirstOrDefault(location => location.Id == id));
    }
}