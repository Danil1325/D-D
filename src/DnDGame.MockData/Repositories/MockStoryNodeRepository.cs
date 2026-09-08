using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.MockData.Repositories;

public class MockStoryNodeRepository : IStoryNodeRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockStoryNodeRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<StoryNode?> GetByIdAsync(int id)
    {
        var node = _store.StoryNodes.FirstOrDefault(n => n.Id == id);
        return Task.FromResult(node is null ? null : GraphHydrator.HydrateNode(_store, node));
    }
}
