using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;

namespace DnDGame.Tests.Domain.Locations;

public class LocationRouteProviderTests
{
    private readonly ILocationRouteProvider _provider = new LocationRouteProvider();

    [Theory]
    [InlineData(RaceType.Human, new[] { LocationId.HerosOverlook, LocationId.MisthavenPort, LocationId.Oakheaven, LocationId.WhisperingWoods, LocationId.Ashtonia, LocationId.TheBonePeaks, LocationId.DarkstormKeep, LocationId.HerosOverlook })]
    [InlineData(RaceType.Elf, new[] { LocationId.HerosOverlook, LocationId.WhisperingWoods, LocationId.MisthavenPort, LocationId.Oakheaven, LocationId.Ashtonia, LocationId.TheBonePeaks, LocationId.DarkstormKeep, LocationId.HerosOverlook })]
    [InlineData(RaceType.Orc, new[] { LocationId.HerosOverlook, LocationId.Ashtonia, LocationId.MisthavenPort, LocationId.Oakheaven, LocationId.WhisperingWoods, LocationId.TheBonePeaks, LocationId.DarkstormKeep, LocationId.HerosOverlook })]
    public void GetRecommendedRoute_ReturnsConfiguredRoute(RaceType raceType, LocationId[] expectedLocations)
    {
        var route = _provider.GetRecommendedRoute(raceType);

        Assert.Equal(expectedLocations, route.Steps.OrderBy(step => step.Order).Select(step => step.LocationId));
        Assert.Equal(6, route.FragmentLocationsFlexibleAfterMainQuestId);
        Assert.Equal(new[] { LocationId.WhisperingWoods, LocationId.Ashtonia, LocationId.TheBonePeaks }, route.FlexibleFragmentLocationIds);
    }

    [Fact]
    public void GetRecommendedRoute_ReturnsDwarfExteriorAndInteriorBonePeakSteps()
    {
        var dwarfRoute = _provider.GetRecommendedRoute(RaceType.Dwarf);

        Assert.Equal(new[]
        {
            LocationId.HerosOverlook, LocationId.TheBonePeaks, LocationId.MisthavenPort,
            LocationId.Oakheaven, LocationId.WhisperingWoods, LocationId.Ashtonia,
            LocationId.TheBonePeaks, LocationId.DarkstormKeep, LocationId.HerosOverlook
        }, dwarfRoute.Steps.OrderBy(step => step.Order).Select(step => step.LocationId));

        Assert.Equal("Exterior", dwarfRoute.Steps.Single(step => step.Order == 2).RouteSegment);
        Assert.Equal("Interior", dwarfRoute.Steps.Single(step => step.Order == 7).RouteSegment);
    }

    [Fact]
    public void GetRecommendedRoute_ReturnsIndependentRouteCopies()
    {
        var first = _provider.GetRecommendedRoute(RaceType.Human);
        first.Steps.Clear();

        var second = _provider.GetRecommendedRoute(RaceType.Human);

        Assert.Equal(8, second.Steps.Count);
    }
}
