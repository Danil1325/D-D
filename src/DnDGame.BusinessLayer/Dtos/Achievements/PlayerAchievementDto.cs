using DnDGame.Domain.Entities.Achievements;

namespace DnDGame.BusinessLayer.Dtos.Achievements;

/// <summary>
/// A catalog entry with the current player's progress toward it.
/// Progress is player identity; the source of truth for a player's row is
/// AchievementProgress, which only ever advances inside the game-event hooks —
/// reading this DTO never changes it.
/// </summary>
public class PlayerAchievementDto : AchievementDto
{
    public int CurrentAmount { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAt { get; init; }

    public static PlayerAchievementDto FromDomain(Achievement achievement, AchievementProgress? progress) => new()
    {
        Id = achievement.Id,
        Code = achievement.Code,
        Title = achievement.Title,
        Description = achievement.Description,
        Type = achievement.Type,
        TargetAmount = achievement.TargetAmount,
        CurrentAmount = progress?.CurrentAmount ?? 0,
        IsCompleted = progress?.CompletedAt is not null,
        CompletedAt = progress?.CompletedAt
    };
}