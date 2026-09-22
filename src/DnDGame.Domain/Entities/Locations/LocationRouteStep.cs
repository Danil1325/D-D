namespace DnDGame.Domain.Entities.Locations;

/// <summary>
/// One ordered point on a location route. Its requirements govern entry into
/// the referenced location.
/// </summary>
public class LocationRouteStep
{
    public int Order { get; set; }
    public LocationId LocationId { get; set; }

    /// <summary>
    /// Optional named area within a location. Dwarves visit The Bone Peaks once
    /// through its Exterior and later through its Interior (Karag-Dur).
    /// </summary>
    public string? RouteSegment { get; set; }
    public LocationUnlockRequirement UnlockRequirement { get; set; } = new();
}
