using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>A player's whole location-progression snapshot: every location's status plus a few overall pointers.</summary>
public class LocationProgressDto
{
    public int PlayerId { get; init; }
    public LocationId? CurrentLocationId { get; init; }
    public LocationId? RecommendedNextLocationId { get; init; }
    public bool HeroOverlookFinaleUnlocked { get; init; }
    public IReadOnlyList<LocationSummaryDto> Locations { get; init; } = Array.Empty<LocationSummaryDto>();
}
