using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Dtos.Scenarios;

/// <summary>
/// Response shape for GET /api/scenario/locations and
/// GET /api/scenario/locations/{locationId}.
/// </summary>
public class LocationDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int RecommendedMinimumLevel { get; init; }
    public string BackgroundImage { get; init; } = string.Empty;
    public bool IsSafeLocation { get; init; }

    public static LocationDto FromDomain(Location location) => new()
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