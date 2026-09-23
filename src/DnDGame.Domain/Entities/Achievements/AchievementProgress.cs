namespace DnDGame.Domain.Entities.Achievements;

/// <summary>
/// Per-player progress toward one Achievement. Composite-keyed
/// (PlayerId, AchievementId), mirroring how LocationProgress is keyed
/// (PlayerId, LocationId) — one row per player per achievement, created on the
/// player's first tracked event and updated afterwards.
///
/// PlayerId is a PlayerCharacter.Id, matching every other per-player table in
/// this store (LocationProgress, and the convention that progress is keyed by
/// the character, not by Account — see Account's remarks: no Account-to-character
/// relationship exists yet).
/// </summary>
public class AchievementProgress
{
    public int PlayerId { get; set; }
    public int AchievementId { get; set; }

    /// <summary>How far along the player is; capped at the achievement's TargetAmount once completed.</summary>
    public int CurrentAmount { get; set; }

    /// <summary>When the target was first reached; null while still in progress.</summary>
    public DateTime? CompletedAt { get; set; }
}
