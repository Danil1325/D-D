using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Characters;

namespace DnDGame.MockData.Repositories;

public class MockCharacterRepository : ICharacterRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockCharacterRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<PlayerCharacter>> GetAllAsync()
    {
        IReadOnlyList<PlayerCharacter> characters = _store.Characters.Select(Hydrate).ToList();
        return Task.FromResult(characters);
    }

    public Task<PlayerCharacter?> GetByIdAsync(int id)
    {
        var character = _store.Characters.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(character is null ? null : Hydrate(character));
    }

    public Task<PlayerCharacter> AddAsync(PlayerCharacter character)
    {
        character.Id = _store.GetNextCharacterId();
        _store.Characters.Add(character);
        return Task.FromResult(character);
    }

    public Task UpdateAsync(PlayerCharacter character)
    {
        // The object passed in is already the exact instance held in _store.Characters
        // (PlayerCharacter is a reference type), so there is nothing further to persist.
        return Task.CompletedTask;
    }

    private PlayerCharacter Hydrate(PlayerCharacter character)
    {
        character.Talents = _store.CharacterTalents
            .Where(t => t.CharacterId == character.Id)
            .Select(t =>
            {
                t.Talent = _store.Talents.FirstOrDefault(x => x.Id == t.TalentId);
                return t;
            })
            .ToList();
        return character;
    }
}
