using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Locations;

namespace DnDGame.MockData.Repositories;

public class MockLocationEncounterRepository : ILocationEncounterRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockLocationEncounterRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<LocationEncounterDefinition?> GetByLocationIdAsync(LocationId locationId)
    {
        var definition = _store.LocationEncounterDefinitions.FirstOrDefault(d => d.LocationId == locationId);
        return Task.FromResult(definition);
    }
}
