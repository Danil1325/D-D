using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;

namespace DnDGame.Tests.Domain.Locations;

public class LocationRouteProviderTests
{
    private readonly ILocationRouteProvider _provider = new LocationRouteProvider();

    [Theory]
    [InlineData(RaceType.Human, new[] { LocationId.HerosOverlook, LocationId.MisthavenPort, LocationId.MisthavenPort, LocationId.Oakheaven, LocationId.Ashtonia, LocationId.WhisperingWoods, LocationId.TheBonePeaks, LocationId.DarkstormKeep, LocationId.HerosOverlook })]
    [InlineData(RaceType.Elf, new[] { LocationId.HerosOverlook, LocationId.WhisperingWoods, LocationId.MisthavenPort, LocationId.Oakheaven, LocationId.Ashtonia, LocationId.WhisperingWoods, LocationId.TheBonePeaks, LocationId.DarkstormKeep, LocationId.HerosOverlook })]
    [InlineData(RaceType.Orc, new[] { LocationId.HerosOverlook, LocationId.Ashtonia, LocationId.MisthavenPort, LocationId.Oakheaven, LocationId.Ashtonia, LocationId.WhisperingWoods, LocationId.TheBonePeaks, LocationId.DarkstormKeep, LocationId.HerosOverlook })]
    [InlineData(RaceType.Dwarf, new[] { LocationId.HerosOverlook, LocationId.TheBonePeaks, LocationId.MisthavenPort, LocationId.Oakheaven, LocationId.Ashtonia, LocationId.WhisperingWoods, LocationId.TheBonePeaks, LocationId.DarkstormKeep, LocationId.HerosOverlook })]
    public void GetRecommendedRoute_ReturnsConfiguredRoute(RaceType raceType, LocationId[] expectedLocations)
    {
        var route = _provider.GetRecommendedRoute(raceType);

        Assert.Equal(expectedLocations, route.Steps.OrderBy(step => step.Order).Select(step => step.LocationId));
        Assert.Equal(Enumerable.Range(1, 9), route.Steps.OrderBy(step => step.Order).Select(step => step.Order));
        Assert.Equal(6, route.FragmentLocationsFlexibleAfterMainQuestId);
        Assert.Equal(new[] { LocationId.Ashtonia, LocationId.WhisperingWoods, LocationId.TheBonePeaks }, route.FlexibleFragmentLocationIds);
    }

    [Fact]
    public void GetRecommendedRoute_DistinguishesOpeningAndFinalHerosOverlookSteps()
    {
        var route = _provider.GetRecommendedRoute(RaceType.Human);

        Assert.Equal(LocationId.HerosOverlook, route.Steps.Single(step => step.Order == 1).LocationId);
        Assert.Equal(LocationId.HerosOverlook, route.Steps.Single(step => step.Order == 9).LocationId);
    }

    [Fact]
    public void GetRecommendedRoute_ReturnsIndependentRouteCopies()
    {
        var first = _provider.GetRecommendedRoute(RaceType.Human);
        first.Steps.Clear();

        var second = _provider.GetRecommendedRoute(RaceType.Human);

        Assert.Equal(9, second.Steps.Count);
    }
}
