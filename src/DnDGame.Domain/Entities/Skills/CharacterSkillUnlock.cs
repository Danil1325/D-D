namespace DnDGame.Domain.Entities.Skills;

/// <summary>
/// Records that a player has unlocked one SkillDefinition. Composite-keyed
/// (PlayerId, SkillDefinitionId), mirroring how AchievementProgress is keyed
/// (PlayerId, AchievementId) — one row per player per unlocked skill, created
/// exactly once when SkillService.UnlockSkillForCurrentPlayerAsync succeeds.
///
/// PlayerId is a PlayerCharacter.Id, matching every other per-player table in
/// this store.
/// </summary>
public class CharacterSkillUnlock
{
    public int PlayerId { get; set; }
    public int SkillDefinitionId { get; set; }
    public DateTime UnlockedAt { get; set; }
}
