using DnDGame.Domain.Entities.Skills;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>The seeded skill-tree catalog (reference data), scoped per (Race, Class).</summary>
public interface ISkillDefinitionRepository
{
    Task<IReadOnlyList<SkillDefinition>> GetByRaceAndClassAsync(int raceId, int classId);
    Task<SkillDefinition?> GetByIdAsync(int id);
}
