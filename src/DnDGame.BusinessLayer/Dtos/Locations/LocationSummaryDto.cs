namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>One catalogue entry as shown on the location-selection screen.</summary>
public class LocationSummaryDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int RecommendedMinimumLevel { get; init; }
    public string BackgroundImage { get; init; } = string.Empty;
    public bool IsSafeLocation { get; init; }
}
