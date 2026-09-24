using DnDGame.Domain.Entities.Skills;

namespace DnDGame.BusinessLayer.Dtos.Skills;

/// <summary>
/// A catalog entry with the current player's unlock state. The source of truth
/// for a player's row is CharacterSkillUnlock, which only ever gets a row from
/// ISkillService.UnlockSkillForCurrentPlayerAsync — reading this DTO never
/// changes it.
/// </summary>
public class PlayerSkillDto : SkillDefinitionDto
{
    public bool IsUnlocked { get; init; }
    public DateTime? UnlockedAt { get; init; }

    public static PlayerSkillDto FromDomain(SkillDefinition skill, CharacterSkillUnlock? unlock) => new()
    {
        Id = skill.Id,
        Code = skill.Code,
        Name = skill.Name,
        Description = skill.Description,
        IconKey = skill.IconKey,
        Category = skill.Category,
        Cost = skill.Cost,
        IsUnlocked = unlock is not null,
        UnlockedAt = unlock?.UnlockedAt
    };
}
