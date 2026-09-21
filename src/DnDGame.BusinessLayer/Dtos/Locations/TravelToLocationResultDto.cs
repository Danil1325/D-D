using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>Outcome of POST /api/locations/travel.</summary>
public class TravelToLocationResultDto
{
    public int PlayerId { get; init; }
    public LocationId LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public LocationStatus Status { get; init; }
    public bool HeroOverlookFinaleUnlocked { get; init; }
    public IReadOnlyCollection<LocationId> AvailableLocationIds { get; init; } = Array.Empty<LocationId>();
    public LocationId? RecommendedNextLocationId { get; init; }
}
