using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>One ordered point on a player's race-recommended route, with that player's current status for it.</summary>
public class LocationRouteStepDto
{
    public int Order { get; init; }
    public LocationId LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string? RouteSegment { get; init; }
    public LocationStatus Status { get; init; }
    public LocationUnlockRequirementDto UnlockRequirements { get; init; } = new();
}

/// <summary>The player's race-recommended journey through the location catalogue.</summary>
public class LocationRouteDto
{
    public RaceType Race { get; init; }
    public IReadOnlyList<LocationRouteStepDto> Steps { get; init; } = Array.Empty<LocationRouteStepDto>();
    public int FragmentLocationsFlexibleAfterMainQuestId { get; init; }
    public IReadOnlyCollection<LocationId> FlexibleFragmentLocationIds { get; init; } = Array.Empty<LocationId>();
}
