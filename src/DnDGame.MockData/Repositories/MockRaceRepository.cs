using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Races;

namespace DnDGame.MockData.Repositories;

public class MockRaceRepository : IRaceRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockRaceRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Race>> GetAllAsync()
    {
        IReadOnlyList<Race> races = _store.Races.Select(Hydrate).ToList();
        return Task.FromResult(races);
    }

    public Task<Race?> GetByIdAsync(int id)
    {
        var race = _store.Races.FirstOrDefault(r => r.Id == id);
        return Task.FromResult(race is null ? null : Hydrate(race));
    }

    private Race Hydrate(Race race)
    {
        race.Traits = _store.RaceTraits.Where(t => t.RaceId == race.Id).ToList();
        race.AttributeRanges = _store.RaceAttributeRanges.Where(r => r.RaceId == race.Id).ToList();
        return race;
    }
}
