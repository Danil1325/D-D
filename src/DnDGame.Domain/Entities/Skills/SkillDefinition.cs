using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Skills;

/// <summary>
/// Reference data — one entry in a (Race, Class) skill tree, seeded once at
/// startup like Card/Achievement and never created or edited by players.
///
/// <see cref="Code"/> is the stable, human-readable identifier the frontend
/// already uses for the same skill (e.g. "human-mage-arcane-adaptation"), kept
/// so both sides stay keyed the same way; <see cref="Id"/> stays a plain seed id
/// so unlock rows can point at it (same convention as Achievement).
///
/// <see cref="Cost"/> is currently flat across every skill (the frontend's own
/// STANDARD_SKILL_COST constant) — there is no RequiredLevel or
/// PrerequisiteSkillId here because the frontend's skillTreeData.ts explicitly
/// marks those as "provisional... until official rules exist". Modeling them
/// now would mean inventing game-design numbers nobody has decided yet; see
/// ISkillService's remarks.
/// </summary>
public class SkillDefinition
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public int RaceId { get; set; }

    public int ClassId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Icon key the frontend maps to a local asset, e.g. "arcane-star".</summary>
    public string IconKey { get; set; } = string.Empty;

    public SkillCategory Category { get; set; }

    /// <summary>Skill points required to unlock. Flat across every skill today (see remarks above).</summary>
    public int Cost { get; set; }
}
