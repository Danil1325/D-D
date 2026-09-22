using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>
/// Read-only catalog details for one world-map location.
/// </summary>
public class LocationDetailsDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int RecommendedMinimumLevel { get; init; }
    public string BackgroundImage { get; init; } = string.Empty;
    public bool IsSafeLocation { get; init; }

    public static LocationDetailsDto FromDomain(Location location) => new()
    {
        Id = location.Id,
        Slug = location.Slug,
        Name = location.Name,
        Description = location.Description,
        RecommendedMinimumLevel = location.RecommendedMinimumLevel,
        BackgroundImage = location.BackgroundImage,
        IsSafeLocation = location.IsSafeLocation
    };
}
