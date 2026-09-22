namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>One ordered point on a player's race-recommended route.</summary>
public class LocationRouteDto
{
    public int Order { get; init; }
    public int LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int RecommendedLevel { get; init; }
    public bool IsCurrent { get; init; }
    public bool IsCompleted { get; init; }
}
