namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>Player-specific location state displayed by the map UI.</summary>
public class LocationStatusDto
{
    public int LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int RecommendedLevel { get; init; }
    public bool IsCurrent { get; init; }
    public bool IsCompleted { get; init; }
}
