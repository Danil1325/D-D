namespace DnDGame.BusinessLayer.Dtos.Achievements;

/// <summary>
/// The full catalog with the player's progress, in one response — the frontend
/// can render every medal and its filled/empty state from this alone.
/// </summary>
public class PlayerAchievementsDto
{
    /// <summary>The PlayerCharacter.Id whose progress this is (see AchievementProgress).</summary>
    public int PlayerId { get; init; }

    public int TotalCount { get; init; }
    public int CompletedCount { get; init; }

    public IReadOnlyList<PlayerAchievementDto> Achievements { get; init; } = Array.Empty<PlayerAchievementDto>();
}