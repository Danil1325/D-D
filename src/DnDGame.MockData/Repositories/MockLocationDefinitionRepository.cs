using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Locations;

namespace DnDGame.MockData.Repositories;

public class MockLocationDefinitionRepository : ILocationDefinitionRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockLocationDefinitionRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<LocationDefinition>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<LocationDefinition>>(_store.LocationDefinitions.ToList());
    }

    public Task<LocationDefinition?> GetByIdAsync(LocationId locationId)
    {
        var definition = _store.LocationDefinitions.FirstOrDefault(d => d.Id == locationId);
        return Task.FromResult(definition);
    }
}
