using DnDGame.API.Controllers;
using DnDGame.BusinessLayer.Dtos.Locations;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Locations;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DnDGame.Tests.API;

public class LocationControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsLocationSummaryArray()
    {
        var controller = new LocationController(new FakeLocationService());

        var response = await controller.GetAll();

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var locations = Assert.IsAssignableFrom<IReadOnlyList<LocationSummaryDto>>(result.Value);
        Assert.Single(locations);
        Assert.Equal("heros-overlook", locations[0].Slug);
    }

    [Fact]
    public async Task GetById_ReturnsLocationDetails()
    {
        var controller = new LocationController(new FakeLocationService());

        var response = await controller.GetById(LocationId.HerosOverlook);

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var details = Assert.IsType<LocationDetailsDto>(result.Value);
        Assert.Equal((int)LocationId.HerosOverlook, details.Id);
    }

    [Fact]
    public async Task GetProgressForPlayer_ReturnsLocationStatusArray()
    {
        var controller = new LocationController(new FakeLocationService());

        var response = await controller.GetProgressForPlayer(playerId: 1);

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var locations = Assert.IsAssignableFrom<IReadOnlyList<LocationStatusDto>>(result.Value);
        Assert.Single(locations);
        Assert.Equal("Current", locations[0].Status);
    }

    [Fact]
    public async Task GetRouteForPlayer_ReturnsLocationRouteArray()
    {
        var controller = new LocationController(new FakeLocationService());

        var response = await controller.GetRouteForPlayer(playerId: 1);

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var route = Assert.IsAssignableFrom<IReadOnlyList<LocationRouteDto>>(result.Value);
        Assert.Single(route);
        Assert.Equal(1, route[0].Order);
    }

    [Fact]
    public async Task GetAvailableEnemies_ReturnsLocationEnemyArray()
    {
        var controller = new LocationController(new FakeLocationService());

        var response = await controller.GetAvailableEnemies(LocationId.HerosOverlook, playerId: 1);

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var enemies = Assert.IsAssignableFrom<IReadOnlyList<LocationEnemyDto>>(result.Value);
        Assert.Single(enemies);
        Assert.Equal("Training Skeleton", enemies[0].Name);
    }

    private sealed class FakeLocationService : ILocationService
    {
        public Task<IReadOnlyList<LocationSummaryDto>> GetAllLocationsAsync()
        {
            IReadOnlyList<LocationSummaryDto> result = new[]
            {
                new LocationSummaryDto
                {
                    Id = (int)LocationId.HerosOverlook,
                    Slug = "heros-overlook",
                    Name = "Hero's Overlook",
                    RecommendedMinimumLevel = 1,
                    BackgroundImage = "Heros_Overlook",
                    IsSafeLocation = true
                }
            };
            return Task.FromResult(result);
        }

        public Task<LocationDetailsDto> GetLocationByIdAsync(LocationId locationId)
        {
            return Task.FromResult(new LocationDetailsDto
            {
                Id = (int)locationId,
                Slug = "heros-overlook",
                Name = "Hero's Overlook",
                Description = "A safe overlook.",
                RecommendedMinimumLevel = 1,
                BackgroundImage = "Heros_Overlook",
                IsSafeLocation = true
            });
        }

        public Task<IReadOnlyList<LocationStatusDto>> GetProgressForPlayerAsync(int playerId)
        {
            IReadOnlyList<LocationStatusDto> result = new[]
            {
                new LocationStatusDto
                {
                    LocationId = (int)LocationId.HerosOverlook,
                    LocationName = "Hero's Overlook",
                    Status = "Current",
                    RecommendedLevel = 1,
                    IsCurrent = true,
                    IsCompleted = false
                }
            };
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<LocationRouteDto>> GetRouteForPlayerAsync(int playerId)
        {
            IReadOnlyList<LocationRouteDto> result = new[]
            {
                new LocationRouteDto
                {
                    Order = 1,
                    LocationId = (int)LocationId.HerosOverlook,
                    LocationName = "Hero's Overlook",
                    Status = "Current",
                    RecommendedLevel = 1,
                    IsCurrent = true,
                    IsCompleted = false
                }
            };
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<LocationEnemyDto>> GetAvailableEnemiesAsync(
            LocationId locationId,
            int playerId,
            string? subLocation = null)
        {
            IReadOnlyList<LocationEnemyDto> result = new[]
            {
                new LocationEnemyDto { Id = 1, Name = "Training Skeleton" }
            };
            return Task.FromResult(result);
        }

        public Task<TravelToLocationResultDto> TravelToLocationAsync(TravelToLocationRequest request)
        {
            throw new NotSupportedException();
        }
    }
}
