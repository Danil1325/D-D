namespace DnDGame.BusinessLayer.Common.Errors;

/// <summary>
/// Error codes specific to the skill-tree unlock flow. Kept separate from the
/// generic ErrorCodes and the other per-feature code classes, following the same
/// per-feature convention as CharacterErrorCodes.
/// </summary>
public static class SkillErrorCodes
{
    /// <summary>The requested skill has already been unlocked by this character.</summary>
    public const string SkillAlreadyUnlocked = "SKILL_ALREADY_UNLOCKED";

    /// <summary>The character does not have enough SkillPoints to unlock the requested skill.</summary>
    public const string InsufficientSkillPoints = "INSUFFICIENT_SKILL_POINTS";
}
