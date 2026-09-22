namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>Full catalogue detail view of one location.</summary>
public class LocationDetailsDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int RecommendedMinimumLevel { get; init; }
    public string BackgroundImage { get; init; } = string.Empty;
    public bool IsSafeLocation { get; init; }
}
