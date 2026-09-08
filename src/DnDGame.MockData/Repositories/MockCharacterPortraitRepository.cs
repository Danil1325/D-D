using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Portraits;

namespace DnDGame.MockData.Repositories;

public class MockCharacterPortraitRepository : ICharacterPortraitRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockCharacterPortraitRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<CharacterPortrait>> GetAllAsync()
    {
        IReadOnlyList<CharacterPortrait> portraits = _store.Portraits.ToList();
        return Task.FromResult(portraits);
    }

    public Task<CharacterPortrait?> GetByRaceAndClassAsync(int raceId, int classId)
    {
        var portrait = _store.Portraits.FirstOrDefault(p => p.RaceId == raceId && p.ClassId == classId);
        return Task.FromResult(portrait);
    }
}
