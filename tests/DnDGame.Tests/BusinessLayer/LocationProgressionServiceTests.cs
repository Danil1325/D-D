using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;

namespace DnDGame.Tests.BusinessLayer;

public class LocationProgressionServiceTests
{
    [Fact]
    public async Task GetAllLocations_ReturnsAllSeededLocations()
    {
        var service = CreateService();

        var locations = await service.GetAllLocationsAsync();

        Assert.Equal(7, locations.Count);
        Assert.All(locations, location =>
        {
            Assert.NotEqual(0, location.Id);
            Assert.False(string.IsNullOrWhiteSpace(location.Slug));
            Assert.False(string.IsNullOrWhiteSpace(location.Name));
            Assert.False(string.IsNullOrWhiteSpace(location.BackgroundImage));
        });
    }

    [Fact]
    public async Task GetLocationDetails_ReturnsTheRequestedLocation()
    {
        var service = CreateService();

        var location = await service.GetLocationDetailsAsync(4);

        Assert.Equal(4, location.Id);
        Assert.Equal("misthaven-port", location.Slug);
        Assert.Equal("Misthaven Port", location.Name);
        Assert.False(string.IsNullOrWhiteSpace(location.Description));
    }

    [Fact]
    public async Task GetLocationDetails_UnknownLocation_FailsWithNotFound()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetLocationDetailsAsync(999));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    private static ILocationProgressionService CreateService()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        return new LocationProgressionService(new MockLocationRepository(store));
    }
}
