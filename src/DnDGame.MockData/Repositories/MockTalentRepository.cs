using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Talents;

namespace DnDGame.MockData.Repositories;

public class MockTalentRepository : ITalentRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockTalentRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Talent>> GetAllAsync()
    {
        IReadOnlyList<Talent> talents = _store.Talents.ToList();
        return Task.FromResult(talents);
    }

    public Task<Talent?> GetByIdAsync(int id)
    {
        var talent = _store.Talents.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(talent);
    }
}
