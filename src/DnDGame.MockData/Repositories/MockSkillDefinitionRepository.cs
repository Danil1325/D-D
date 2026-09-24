using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Skills;

namespace DnDGame.MockData.Repositories;

public class MockSkillDefinitionRepository : ISkillDefinitionRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockSkillDefinitionRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<SkillDefinition>> GetByRaceAndClassAsync(int raceId, int classId)
    {
        var rows = _store.SkillDefinitions
            .Where(skill => skill.RaceId == raceId && skill.ClassId == classId)
            .ToList();
        return Task.FromResult<IReadOnlyList<SkillDefinition>>(rows);
    }

    public Task<SkillDefinition?> GetByIdAsync(int id)
    {
        var skill = _store.SkillDefinitions.FirstOrDefault(candidate => candidate.Id == id);
        return Task.FromResult(skill);
    }
}
