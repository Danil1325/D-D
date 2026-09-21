using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Locations;

/// <summary>Central, immutable-in-practice configuration for race location routes.</summary>
public static class LocationRouteRegistry
{
    private static readonly IReadOnlyDictionary<RaceType, RaceLocationRoute> Routes =
        new Dictionary<RaceType, RaceLocationRoute>
        {
            [RaceType.Human] = CreateRoute(RaceType.Human,
                (LocationId.HerosOverlook, null),
                (LocationId.MisthavenPort, null),
                (LocationId.Oakheaven, null),
                (LocationId.WhisperingWoods, null),
                (LocationId.Ashtonia, null),
                (LocationId.TheBonePeaks, null),
                (LocationId.DarkstormKeep, null),
                (LocationId.HerosOverlook, null)),
            [RaceType.Elf] = CreateRoute(RaceType.Elf,
                (LocationId.HerosOverlook, null),
                (LocationId.WhisperingWoods, null),
                (LocationId.MisthavenPort, null),
                (LocationId.Oakheaven, null),
                (LocationId.Ashtonia, null),
                (LocationId.TheBonePeaks, null),
                (LocationId.DarkstormKeep, null),
                (LocationId.HerosOverlook, null)),
            [RaceType.Orc] = CreateRoute(RaceType.Orc,
                (LocationId.HerosOverlook, null),
                (LocationId.Ashtonia, null),
                (LocationId.MisthavenPort, null),
                (LocationId.Oakheaven, null),
                (LocationId.WhisperingWoods, null),
                (LocationId.TheBonePeaks, null),
                (LocationId.DarkstormKeep, null),
                (LocationId.HerosOverlook, null)),
            [RaceType.Dwarf] = CreateRoute(RaceType.Dwarf,
                (LocationId.HerosOverlook, null),
                (LocationId.TheBonePeaks, "Exterior"),
                (LocationId.MisthavenPort, null),
                (LocationId.Oakheaven, null),
                (LocationId.WhisperingWoods, null),
                (LocationId.Ashtonia, null),
                (LocationId.TheBonePeaks, "Interior"),
                (LocationId.DarkstormKeep, null),
                (LocationId.HerosOverlook, null))
        };

    public static RaceLocationRoute Get(RaceType raceType)
    {
        if (!Routes.TryGetValue(raceType, out var route))
            throw new ArgumentOutOfRangeException(nameof(raceType), raceType, "No location route is configured for this race.");

        return Copy(route);
    }

    private static RaceLocationRoute CreateRoute(
        RaceType raceType, params (LocationId LocationId, string? Segment)[] locations) => new()
    {
        RaceType = raceType,
        Steps = locations.Select((location, index) => new LocationRouteStep
        {
            Order = index + 1,
            LocationId = location.LocationId,
            RouteSegment = location.Segment
        }).ToList(),
        FragmentLocationsFlexibleAfterMainQuestId = 6,
        FlexibleFragmentLocationIds = new List<LocationId>
        {
            LocationId.WhisperingWoods,
            LocationId.Ashtonia,
            LocationId.TheBonePeaks
        }
    };

    private static RaceLocationRoute Copy(RaceLocationRoute source) => new()
    {
        RaceType = source.RaceType,
        FragmentLocationsFlexibleAfterMainQuestId = source.FragmentLocationsFlexibleAfterMainQuestId,
        FlexibleFragmentLocationIds = source.FlexibleFragmentLocationIds.ToList(),
        Steps = source.Steps.Select(step => new LocationRouteStep
        {
            Order = step.Order,
            LocationId = step.LocationId,
            RouteSegment = step.RouteSegment,
            UnlockRequirement = new LocationUnlockRequirement
            {
                MinimumLevel = step.UnlockRequirement.MinimumLevel,
                RequiredRace = step.UnlockRequirement.RequiredRace,
                RequiredQuestIds = step.UnlockRequirement.RequiredQuestIds.ToList(),
                RequiredStoryFlags = new Dictionary<string, bool>(step.UnlockRequirement.RequiredStoryFlags),
                RequiredFragmentCount = step.UnlockRequirement.RequiredFragmentCount,
                RequiredPreviousLocationIds = step.UnlockRequirement.RequiredPreviousLocationIds.ToList()
            }
        }).ToList()
    };
}
