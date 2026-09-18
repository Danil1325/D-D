using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.MockData.Repositories;

public class MockQuestRepository : IQuestRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockQuestRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Quest>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<Quest>>(_store.Quests.ToList());
    }

    public Task<Quest?> GetByIdAsync(int id)
    {
        return Task.FromResult(_store.Quests.FirstOrDefault(q => q.Id == id));
    }
}