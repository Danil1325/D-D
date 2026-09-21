using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>Full detail view of one location, projected for a specific player.</summary>
public class LocationDetailsDto
{
    public LocationId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string BackgroundImage { get; init; } = string.Empty;
    public LocationStatus Status { get; init; }
    public int RecommendedMinimumLevel { get; init; }
    public int RecommendedMaximumLevel { get; init; }
    public bool IsSafeLocation { get; init; }
    public LocationUnlockRequirementDto UnlockRequirements { get; init; } = new();
    public bool IsCurrent { get; init; }
    public bool IsRecommendedNext { get; init; }
    public bool Visited { get; init; }
    public bool Completed { get; init; }
    public int? UnlockedAtLevel { get; init; }
    public IReadOnlyCollection<int> MainQuestIds { get; init; } = Array.Empty<int>();
    public IReadOnlyCollection<int> SideQuestIds { get; init; } = Array.Empty<int>();
    public IReadOnlyCollection<string> SpecialFlags { get; init; } = Array.Empty<string>();
}
