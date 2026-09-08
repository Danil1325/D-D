using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.MockData.Repositories;

public class MockAdventureRepository : IAdventureRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockAdventureRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Adventure>> GetAllAsync()
    {
        IReadOnlyList<Adventure> adventures = _store.Adventures.Select(Hydrate).ToList();
        return Task.FromResult(adventures);
    }

    public Task<Adventure?> GetByIdAsync(int id)
    {
        var adventure = _store.Adventures.FirstOrDefault(a => a.Id == id);
        return Task.FromResult(adventure is null ? null : Hydrate(adventure));
    }

    private Adventure Hydrate(Adventure adventure)
    {
        adventure.Nodes = _store.StoryNodes
            .Where(n => n.AdventureId == adventure.Id)
            .Select(n => GraphHydrator.HydrateNode(_store, n))
            .ToList();

        if (adventure.StartingNodeId.HasValue)
        {
            adventure.StartingNode = adventure.Nodes.FirstOrDefault(n => n.Id == adventure.StartingNodeId.Value);
        }

        return adventure;
    }
}
