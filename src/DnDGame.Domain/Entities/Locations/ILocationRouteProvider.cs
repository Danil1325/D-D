using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Locations;

public interface ILocationRouteProvider
{
    RaceLocationRoute GetRecommendedRoute(RaceType raceType);
}
