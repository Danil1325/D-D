namespace DnDGame.Domain.Enums;

/// <summary>
/// The skill-tree category a skill belongs to, mirroring the frontend's
/// SkillCategoryId ('combat' | 'defense' | 'magic' | 'survival' | 'utility').
/// Presentation grouping only — carries no mechanical effect of its own.
/// </summary>
public enum SkillCategory
{
    Combat = 1,
    Defense = 2,
    Magic = 3,
    Survival = 4,
    Utility = 5
}
