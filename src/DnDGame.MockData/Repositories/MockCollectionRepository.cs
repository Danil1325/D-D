using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Collection;

namespace DnDGame.MockData.Repositories;

public class MockCollectionRepository : ICollectionRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockCollectionRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<CollectionEntry>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<CollectionEntry>>(_store.CollectionEntries.ToList());
    }
}
