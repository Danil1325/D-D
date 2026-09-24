using DnDGame.BusinessLayer.Dtos.Skills;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Read + unlock side of the skill-tree feature. Skills are scoped by
/// (Race, Class) — see SkillDefinition. Unlock cost is currently flat across
/// every skill and there is no level or prerequisite gating: the frontend's own
/// skillTreeData.ts marks RequiredLevel/PrerequisiteSkillId as "provisional...
/// until official rules exist", so this service deliberately does not invent
/// them — it only validates that the skill belongs to the character's
/// (Race, Class) tree, isn't already unlocked, and that the character can afford
/// it. Unlock spends the character's existing SkillPoints balance
/// (PlayerCharacter.SkillPoints), the same counter the level-up flow already
/// increments.
/// </summary>
public interface ISkillService
{
    /// <summary>
    /// The current player's full skill tree: their (Race, Class) catalog merged
    /// with their unlock state. Resolves the player server-side via
    /// ICurrentPlayerService. Throws DomainException(NOT_FOUND) if the current
    /// player has no character yet.
    /// </summary>
    Task<CharacterSkillsDto> GetSkillTreeForCurrentPlayerAsync();

    /// <summary>
    /// Unlocks one skill for the current player and returns their updated skill
    /// tree. Throws DomainException(NOT_FOUND) if the skill doesn't exist or
    /// doesn't belong to the character's (Race, Class), SKILL_ALREADY_UNLOCKED if
    /// already unlocked, or INSUFFICIENT_SKILL_POINTS if the character can't
    /// afford it.
    /// </summary>
    Task<CharacterSkillsDto> UnlockSkillForCurrentPlayerAsync(int skillDefinitionId);
}
