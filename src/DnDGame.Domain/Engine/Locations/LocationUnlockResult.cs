using DnDGame.Domain.Entities.Locations;

namespace DnDGame.Domain.Engine.Locations;

/// <summary>Outcome data for a location progression operation.</summary>
public class LocationUnlockResult
{
    public int PlayerId { get; init; }
    public LocationId LocationId { get; init; }
    public LocationStatus Status { get; init; }
    public bool HeroOverlookFinaleUnlocked { get; init; }
    public IReadOnlyList<LocationId> AvailableLocationIds { get; init; } = Array.Empty<LocationId>();
    public LocationId? RecommendedNextLocationId { get; init; }
}
