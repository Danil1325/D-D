using DnDGame.Domain.Entities.Achievements;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Dtos.Achievements;

/// <summary>One catalog entry, independent of any player's progress.</summary>
public class AchievementDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public AchievementType Type { get; init; }
    public int TargetAmount { get; init; }

    public static AchievementDto FromDomain(Achievement achievement) => new()
    {
        Id = achievement.Id,
        Code = achievement.Code,
        Title = achievement.Title,
        Description = achievement.Description,
        Type = achievement.Type,
        TargetAmount = achievement.TargetAmount
    };
}