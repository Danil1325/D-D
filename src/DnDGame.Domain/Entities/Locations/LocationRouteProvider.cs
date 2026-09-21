using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Locations;

/// <summary>Provides the configured recommended route for a character race.</summary>
public class LocationRouteProvider : ILocationRouteProvider
{
    public RaceLocationRoute GetRecommendedRoute(RaceType raceType) =>
        LocationRouteRegistry.Get(raceType);
}
