using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>
/// Stable catalog data for the locations list.
/// </summary>
public class LocationSummaryDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int RecommendedMinimumLevel { get; init; }
    public string BackgroundImage { get; init; } = string.Empty;
    public bool IsSafeLocation { get; init; }

    public static LocationSummaryDto FromDomain(Location location) => new()
    {
        Id = location.Id,
        Slug = location.Slug,
        Name = location.Name,
        RecommendedMinimumLevel = location.RecommendedMinimumLevel,
        BackgroundImage = location.BackgroundImage,
        IsSafeLocation = location.IsSafeLocation
    };
}
