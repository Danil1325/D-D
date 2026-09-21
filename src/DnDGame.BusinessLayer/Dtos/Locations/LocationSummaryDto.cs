using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>
/// One catalogue entry as shown on the location-selection screen, projected for a
/// specific player. Deliberately excludes encounter data — GET /api/locations/
/// {locationId}/enemies is the only endpoint allowed to reveal available encounters,
/// and only for the requesting player.
/// </summary>
public class LocationSummaryDto
{
    public LocationId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string BackgroundImage { get; init; } = string.Empty;
    public LocationStatus Status { get; init; }
    public int RecommendedMinimumLevel { get; init; }
    public int RecommendedMaximumLevel { get; init; }
    public LocationUnlockRequirementDto UnlockRequirements { get; init; } = new();
    public bool IsCurrent { get; init; }
    public bool IsRecommendedNext { get; init; }
}
