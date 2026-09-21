namespace DnDGame.Domain.Entities.Locations;

/// <summary>
/// One ordered point on a location route. Its requirements govern entry into
/// the referenced location.
/// </summary>
public class LocationRouteStep
{
    public int Order { get; set; }
    public LocationId LocationId { get; set; }
    public LocationUnlockRequirement UnlockRequirement { get; set; } = new();
}
