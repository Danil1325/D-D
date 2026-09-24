using DnDGame.Domain.Entities.Skills;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Dtos.Skills;

/// <summary>One catalog entry, independent of any player's unlock state.</summary>
public class SkillDefinitionDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string IconKey { get; init; } = string.Empty;
    public SkillCategory Category { get; init; }
    public int Cost { get; init; }

    public static SkillDefinitionDto FromDomain(SkillDefinition skill) => new()
    {
        Id = skill.Id,
        Code = skill.Code,
        Name = skill.Name,
        Description = skill.Description,
        IconKey = skill.IconKey,
        Category = skill.Category,
        Cost = skill.Cost
    };
}
