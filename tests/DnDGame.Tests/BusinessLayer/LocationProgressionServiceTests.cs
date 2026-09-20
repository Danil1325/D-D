using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Locations;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Engine.Scenario;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;

namespace DnDGame.Tests.BusinessLayer;

public class LocationProgressionServiceTests
{
    private const int PlayerId = 42;
    private const int CharacterId = 7;
    private const int SessionId = 11;
    private const int StartingLocationId = 10;
    private const int ReachableLocationId = 20;
    private const int UnreachableLocationId = 30;
    private const int RaceGatedLocationId = 40;
    private const int CurrentSceneId = 100;
    private const int SameLocationSceneId = 150;
    private const int ReachableSceneId = 200;
    private const int RaceGatedSceneId = 300;

    [Fact]
    public async Task GetAllLocations_ReturnsAllSeededLocations()
    {
        var service = CreateSeededService();

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
        var service = CreateSeededService();

        var location = await service.GetLocationDetailsAsync(4);

        Assert.Equal(4, location.Id);
        Assert.Equal("misthaven-port", location.Slug);
        Assert.Equal("Misthaven Port", location.Name);
        Assert.False(string.IsNullOrWhiteSpace(location.Description));
    }

    [Fact]
    public async Task GetLocationDetails_UnknownLocation_FailsWithNotFound()
    {
        var service = CreateSeededService();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetLocationDetailsAsync(999));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task TravelToLocation_ReachableDestination_AdvancesAndReturnsCurrentLocationAndScene()
    {
        var store = CreateTravelStore();
        var service = CreateService(store);

        var result = await service.TravelToLocationAsync(new TravelToLocationRequestDto
        {
            PlayerId = PlayerId,
            LocationId = ReachableLocationId
        });

        Assert.Equal(ReachableLocationId, result.CurrentLocation.Id);
        Assert.Equal(ReachableSceneId, result.CurrentScene.Id);
        Assert.Equal(ReachableLocationId, result.CurrentScene.LocationId);
        Assert.Equal(ReachableSceneId, store.ScenarioProgresses.Single().CurrentSceneId);
    }

    [Fact]
    public async Task TravelToLocation_UnreachableLocation_FailsWithConflict()
    {
        var store = CreateTravelStore();
        var service = CreateService(store);

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            service.TravelToLocationAsync(new TravelToLocationRequestDto
            {
                PlayerId = PlayerId,
                LocationId = UnreachableLocationId
            }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    [Fact]
    public async Task TravelToLocation_RaceGatedTransition_CannotBeBypassed()
    {
        var store = CreateTravelStore();
        var service = CreateService(store);

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            service.TravelToLocationAsync(new TravelToLocationRequestDto
            {
                PlayerId = PlayerId,
                LocationId = RaceGatedLocationId
            }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
        Assert.Equal(CurrentSceneId, store.ScenarioProgresses.Single().CurrentSceneId);
    }

    [Fact]
    public async Task TravelToLocation_CurrentLocation_FailsWithConflictAndLeavesCurrentSceneUnchanged()
    {
        var store = CreateTravelStore();
        var service = CreateService(store);

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            service.TravelToLocationAsync(new TravelToLocationRequestDto
            {
                PlayerId = PlayerId,
                LocationId = StartingLocationId
            }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
        Assert.Equal(CurrentSceneId, store.ScenarioProgresses.Single().CurrentSceneId);
    }

    [Fact]
    public async Task TravelToLocation_CurrentSceneChangesOnlyAfterValidTravel()
    {
        var store = CreateTravelStore();
        var service = CreateService(store);
        var progress = store.ScenarioProgresses.Single();

        Assert.Equal(CurrentSceneId, progress.CurrentSceneId);

        await Assert.ThrowsAsync<DomainException>(() =>
            service.TravelToLocationAsync(new TravelToLocationRequestDto
            {
                PlayerId = PlayerId,
                LocationId = UnreachableLocationId
            }));

        Assert.Equal(CurrentSceneId, progress.CurrentSceneId);

        await service.TravelToLocationAsync(new TravelToLocationRequestDto
        {
            PlayerId = PlayerId,
            LocationId = ReachableLocationId
        });

        Assert.Equal(ReachableSceneId, progress.CurrentSceneId);
    }

    [Fact]
    public async Task TravelToLocation_FailedTravel_LeavesCurrentProgressUnchanged()
    {
        var store = CreateTravelStore();
        var service = CreateService(store);

        await Assert.ThrowsAsync<DomainException>(() =>
            service.TravelToLocationAsync(new TravelToLocationRequestDto
            {
                PlayerId = PlayerId,
                LocationId = UnreachableLocationId
            }));

        Assert.Equal(CurrentSceneId, store.ScenarioProgresses.Single().CurrentSceneId);
    }

    private static ILocationProgressionService CreateSeededService()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        return CreateService(store);
    }

    private static ILocationProgressionService CreateService(InMemoryGameDataStore store)
    {
        var locationRepository = new MockLocationRepository(store);
        var storySceneRepository = new MockStorySceneRepository(store);
        var scenarioService = new ScenarioService(
            new ScenarioEngine(),
            new MockCharacterRepository(store),
            new MockGameSessionRepository(store),
            new MockScenarioProgressRepository(store),
            storySceneRepository,
            new MockQuestRepository(store),
            new MockPlayerQuestRepository(store),
            locationRepository);

        return new LocationProgressionService(
            locationRepository,
            scenarioService,
            storySceneRepository);
    }

    private static InMemoryGameDataStore CreateTravelStore()
    {
        var store = new InMemoryGameDataStore();
        store.Locations.AddRange(new[]
        {
            Location(StartingLocationId, "Current"),
            Location(ReachableLocationId, "Reachable"),
            Location(UnreachableLocationId, "Unreachable"),
            Location(RaceGatedLocationId, "Race gated")
        });

        store.StoryScenes.AddRange(new[]
        {
            Scene(
                CurrentSceneId,
                StartingLocationId,
                "Current scene",
                new List<StoryChoice>
                {
                    Choice(1, "Go to reachable", ReachableSceneId),
                    Choice(2, "Go to race gated", RaceGatedSceneId, requiredRaceId: 99),
                    Choice(3, "Go to another current-location scene", SameLocationSceneId)
                }),
            Scene(SameLocationSceneId, StartingLocationId, "Same location scene"),
            Scene(ReachableSceneId, ReachableLocationId, "Reachable scene"),
            Scene(RaceGatedSceneId, RaceGatedLocationId, "Race gated scene"),
            Scene(400, UnreachableLocationId, "Unreachable scene")
        });

        store.Characters.Add(new PlayerCharacter
        {
            Id = CharacterId,
            OwnerId = PlayerId.ToString(),
            Name = "Tester",
            RaceId = 1,
            ClassId = 1,
            Level = 1
        });

        store.GameSessions.Add(new GameSession
        {
            Id = SessionId,
            CharacterId = CharacterId,
            AdventureId = 1,
            Status = GameSessionStatus.InProgress,
            StartedAt = DateTime.UtcNow
        });

        store.ScenarioProgresses.Add(new ScenarioProgress
        {
            Id = 1,
            GameSessionId = SessionId,
            CurrentSceneId = CurrentSceneId
        });

        return store;
    }

    private static Location Location(int id, string name)
    {
        return new Location
        {
            Id = id,
            Slug = name.ToLowerInvariant().Replace(' ', '-'),
            Name = name,
            Description = $"{name} description",
            RecommendedMinimumLevel = 1,
            BackgroundImage = $"{name}-background",
            IsSafeLocation = true
        };
    }

    private static StoryScene Scene(
        int id,
        int locationId,
        string title,
        ICollection<StoryChoice>? choices = null)
    {
        return new StoryScene
        {
            Id = id,
            Act = 1,
            Chapter = 1,
            Title = title,
            LocationId = locationId,
            BackgroundImage = "scene-background",
            Choices = choices ?? new List<StoryChoice>()
        };
    }

    private static StoryChoice Choice(int id, string text, int nextSceneId, int? requiredRaceId = null)
    {
        var choice = new StoryChoice
        {
            Id = id,
            Text = text,
            NextSceneId = nextSceneId
        };

        if (requiredRaceId.HasValue)
        {
            choice.Requirements.Add(new ChoiceRequirement { RaceId = requiredRaceId.Value });
        }

        return choice;
    }
}
