using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Classes;

namespace DnDGame.MockData.Repositories;

public class MockClassRepository : IClassRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockClassRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<CharacterClass>> GetAllAsync()
    {
        IReadOnlyList<CharacterClass> classes = _store.Classes.Select(Hydrate).ToList();
        return Task.FromResult(classes);
    }

    public Task<CharacterClass?> GetByIdAsync(int id)
    {
        var characterClass = _store.Classes.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(characterClass is null ? null : Hydrate(characterClass));
    }

    private CharacterClass Hydrate(CharacterClass characterClass)
    {
        characterClass.Features = _store.ClassFeatures
            .Where(f => f.ClassId == characterClass.Id)
            .ToList();
        return characterClass;
    }
}
