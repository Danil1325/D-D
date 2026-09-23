namespace DnDGame.BusinessLayer.Dtos.Achievements;

/// <summary>
/// What the achievements screen renders for the current player after the game has
/// been resumed: the whole catalog split into three state buckets. Each entry is a
/// <see cref="PlayerAchievementDto"/>, which carries the catalog fields (code,
/// title, description, type, target) alongside the player's progress.
///
/// The player is resolved server-side via ICurrentPlayerService exactly like
/// GET /api/character/current — the client never sends a player id, so this can be
/// fetched on every load after a page refresh. 404 is produced by the global
/// NOT_FOUND middleware when the current player has no character yet.
/// </summary>
public class AchievementsOverviewDto
{
    /// <summary>The PlayerCharacter.Id whose progress this is (see AchievementProgress).</summary>
    public int PlayerId { get; init; }

    public int TotalCount { get; init; }
    public int CompletedCount { get; init; }

    /// <summary>Not started yet: no progress row, so the entry renders greyed out.</summary>
    public IReadOnlyList<PlayerAchievementDto> Locked { get; init; } = Array.Empty<PlayerAchievementDto>();

    /// <summary>Has progress but has not reached the target yet: renders with a progress bar.</summary>
    public IReadOnlyList<PlayerAchievementDto> InProgress { get; init; } = Array.Empty<PlayerAchievementDto>();

    /// <summary>Target reached; CompletedAt holds the unlock timestamp.</summary>
    public IReadOnlyList<PlayerAchievementDto> Unlocked { get; init; } = Array.Empty<PlayerAchievementDto>();
}