namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>
/// One ordered step in the player's authored story route.
/// </summary>
public class LocationRouteStepDto
{
    public int Order { get; init; }
    public int LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int RecommendedLevel { get; init; }
    public bool IsCurrent { get; init; }
    public bool IsCompleted { get; init; }
}
