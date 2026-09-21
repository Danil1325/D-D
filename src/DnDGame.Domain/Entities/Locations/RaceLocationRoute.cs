using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Locations;

/// <summary>A race-specific recommended journey through The Crown of Ash.</summary>
public class RaceLocationRoute
{
    public RaceType RaceType { get; set; }
    public ICollection<LocationRouteStep> Steps { get; set; } = new List<LocationRouteStep>();

    /// <summary>
    /// From this completed main-quest number onward, the remaining Crown
    /// Fragment locations are recommendations rather than a fixed route.
    /// </summary>
    public int FragmentLocationsFlexibleAfterMainQuestId { get; set; } = 6;

    public ICollection<LocationId> FlexibleFragmentLocationIds { get; set; } = new List<LocationId>();
}
