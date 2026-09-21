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
    private const int HerosOverlookLocationId = 3;
    private const int AshtoniaLocationId = 1;
    private const int DarkstormKeepLocationId = 2;
    private const int MisthavenPortLocationId = 4;
    private const int OakheavenLocationId = 5;
    private const int TheBonePeaksLocationId = 6;
    private const int WhisperingWoodsLocationId = 7;

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
    public async Task GetLocationEnemies_DarkstormKeep_ReturnsDemonSkeletonAndWraithTemplates()
    {
        var service = CreateService(CreateRouteStore(raceId: 1));

        var enemies = await service.GetLocationEnemiesAsync(DarkstormKeepLocationId, PlayerId);

        AssertEnemyIds(enemies, 1, 2, 3, 16, 17, 18, 19, 20, 21);
    }

    [Fact]
    public async Task GetLocationEnemies_BonePeaks_ReturnsSkeletonTrollAndChimeraTemplates()
    {
        var service = CreateService(CreateRouteStore(raceId: 1));

        var enemies = await service.GetLocationEnemiesAsync(TheBonePeaksLocationId, PlayerId);

        AssertEnemyIds(enemies, 1, 2, 3, 10, 11, 12, 13, 14, 15);
    }

    [Fact]
    public async Task GetLocationEnemies_WhisperingWoods_ReturnsGoblinSlimeAndWraithTemplates()
    {
        var service = CreateService(CreateRouteStore(raceId: 1));

        var enemies = await service.GetLocationEnemiesAsync(WhisperingWoodsLocationId, PlayerId);

        AssertEnemyIds(enemies, 4, 5, 6, 7, 8, 9, 19, 20, 21);
    }

    [Fact]
    public async Task GetLocationEnemies_SafeLocationWithNoEnemyTypes_ReturnsEmpty()
    {
        var service = CreateService(CreateRouteStore(raceId: 1));

        var enemies = await service.GetLocationEnemiesAsync(HerosOverlookLocationId, PlayerId);

        Assert.Empty(enemies);
    }

    [Fact]
    public async Task GetLocationEnemies_InvalidLocationId_FailsWithValidationError()
    {
        var service = CreateService(CreateRouteStore(raceId: 1));

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            service.GetLocationEnemiesAsync(0, PlayerId));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
    }

    [Fact]
    public async Task GetLocationEnemies_UnknownLocation_FailsWithNotFound()
    {
        var service = CreateService(CreateRouteStore(raceId: 1));

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            service.GetLocationEnemiesAsync(999, PlayerId));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task GetLocationEnemies_InvalidPlayerId_FailsWithValidationError()
    {
        var service = CreateService(CreateRouteStore(raceId: 1));

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            service.GetLocationEnemiesAsync(DarkstormKeepLocationId, 0));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
    }

    [Fact]
    public async Task GetLocationEnemies_UnknownPlayer_FailsWithNotFound()
    {
        var service = CreateService(CreateRouteStore(raceId: 1));

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            service.GetLocationEnemiesAsync(DarkstormKeepLocationId, 999));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task GetLocationEnemies_DuplicateEnemyMatches_AreReturnedOnce()
    {
        var store = CreateRouteStore(raceId: 1);
        store.Enemies.Add(store.Enemies.Single(enemy => enemy.Id == 16));
        var service = CreateService(store);

        var enemies = await service.GetLocationEnemiesAsync(DarkstormKeepLocationId, PlayerId);

        AssertEnemyIds(enemies, 1, 2, 3, 16, 17, 18, 19, 20, 21);
        Assert.Equal(enemies.Count, enemies.Select(enemy => enemy.Id).Distinct().Count());
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

    [Fact]
    public async Task GetPlayerRoute_Human_ReturnsAuthoredRoute()
    {
        var service = CreateService(CreateRouteStore(raceId: 1));

        var route = await service.GetPlayerRouteAsync(PlayerId);

        AssertRoute(route, new[]
        {
            HerosOverlookLocationId,
            MisthavenPortLocationId,
            MisthavenPortLocationId,
            OakheavenLocationId,
            AshtoniaLocationId,
            WhisperingWoodsLocationId,
            TheBonePeaksLocationId,
            DarkstormKeepLocationId,
            HerosOverlookLocationId
        });
    }

    [Fact]
    public async Task GetPlayerRoute_Elf_ReturnsAuthoredRoute()
    {
        var service = CreateService(CreateRouteStore(raceId: 2));

        var route = await service.GetPlayerRouteAsync(PlayerId);

        AssertRoute(route, new[]
        {
            HerosOverlookLocationId,
            WhisperingWoodsLocationId,
            MisthavenPortLocationId,
            OakheavenLocationId,
            AshtoniaLocationId,
            WhisperingWoodsLocationId,
            TheBonePeaksLocationId,
            DarkstormKeepLocationId,
            HerosOverlookLocationId
        });
    }

    [Fact]
    public async Task GetPlayerRoute_Orc_ReturnsAuthoredRoute()
    {
        var service = CreateService(CreateRouteStore(raceId: 3));

        var route = await service.GetPlayerRouteAsync(PlayerId);

        AssertRoute(route, new[]
        {
            HerosOverlookLocationId,
            AshtoniaLocationId,
            MisthavenPortLocationId,
            OakheavenLocationId,
            AshtoniaLocationId,
            WhisperingWoodsLocationId,
            TheBonePeaksLocationId,
            DarkstormKeepLocationId,
            HerosOverlookLocationId
        });
    }

    [Fact]
    public async Task GetPlayerRoute_Dwarf_ReturnsAuthoredRoute()
    {
        var service = CreateService(CreateRouteStore(raceId: 4));

        var route = await service.GetPlayerRouteAsync(PlayerId);

        AssertRoute(route, new[]
        {
            HerosOverlookLocationId,
            TheBonePeaksLocationId,
            MisthavenPortLocationId,
            OakheavenLocationId,
            AshtoniaLocationId,
            WhisperingWoodsLocationId,
            TheBonePeaksLocationId,
            DarkstormKeepLocationId,
            HerosOverlookLocationId
        });
    }

    [Fact]
    public async Task GetPlayerRoute_HumanRaceSpecificMisthaven_IsOrderTwo()
    {
        var service = CreateService(CreateRouteStore(raceId: 1, currentSceneId: 1203));

        var route = await service.GetPlayerRouteAsync(PlayerId);

        AssertCurrentStep(route, order: 2, locationId: MisthavenPortLocationId);
    }

    [Fact]
    public async Task GetPlayerRoute_HumanPostConvergenceMisthaven_IsOrderThree()
    {
        var service = CreateService(CreateRouteStore(raceId: 1, currentSceneId: 2003));

        var route = await service.GetPlayerRouteAsync(PlayerId);

        AssertCurrentStep(route, order: 3, locationId: MisthavenPortLocationId);
    }

    [Fact]
    public async Task GetPlayerRoute_RaceBranchInterstitial_DoesNotMarkPostRaceMisthavenProgress()
    {
        var service = CreateService(CreateRouteStore(raceId: 1, currentSceneId: 1205));

        var route = await service.GetPlayerRouteAsync(PlayerId);

        AssertRouteProgress(route, currentOrder: null, completedThroughOrder: 1);
        Assert.Equal("upcoming", Assert.Single(route, step => step.Order == 2).Status);
        Assert.Equal("upcoming", Assert.Single(route, step => step.Order == 3).Status);
    }

    [Fact]
    public async Task GetPlayerRoute_ActFourGatheringInterstitial_DoesNotRegressBelowOakheaven()
    {
        var service = CreateService(CreateRouteStore(raceId: 1, currentSceneId: 4100));

        var route = await service.GetPlayerRouteAsync(PlayerId);

        AssertRouteProgress(route, currentOrder: null, completedThroughOrder: 4);
        Assert.Equal("completed", Assert.Single(route, step => step.Order == 4).Status);
        Assert.Equal("upcoming", Assert.Single(route, step => step.Order == 5).Status);
    }

    [Theory]
    [InlineData(2, 4009, 6, WhisperingWoodsLocationId)]
    [InlineData(3, 4008, 5, AshtoniaLocationId)]
    [InlineData(4, 4010, 7, TheBonePeaksLocationId)]
    public async Task GetPlayerRoute_RepeatedRaceLocationFragmentScenes_ResolveToFragmentStep(
        int raceId,
        int currentSceneId,
        int expectedOrder,
        int expectedLocationId)
    {
        var service = CreateService(CreateRouteStore(raceId, currentSceneId));

        var route = await service.GetPlayerRouteAsync(PlayerId);

        AssertCurrentStep(route, expectedOrder, expectedLocationId);
    }

    [Fact]
    public async Task GetPlayerRoute_AuthoredProgression_IsMonotonicThroughInterstitials()
    {
        var progression = new (int SceneId, int? CurrentOrder, int CompletedThroughOrder)[]
        {
            (900, 1, 0),
            (1205, null, 1),
            (1203, 2, 1),
            (2003, 3, 2),
            (3006, 4, 3),
            (4100, null, 4),
            (4008, 5, 4)
        };
        var previousRouteCursor = 0;

        foreach (var (sceneId, currentOrder, completedThroughOrder) in progression)
        {
            var service = CreateService(CreateRouteStore(raceId: 1, currentSceneId: sceneId));

            var route = await service.GetPlayerRouteAsync(PlayerId);

            AssertRouteProgress(route, currentOrder, completedThroughOrder);
            var routeCursor = currentOrder ?? completedThroughOrder;
            Assert.True(routeCursor >= previousRouteCursor);
            previousRouteCursor = routeCursor;
        }
    }

    [Fact]
    public async Task GetPlayerRoute_HerosOverlookOpeningAndFinalReturn_AreDistinctSteps()
    {
        var openingService = CreateService(CreateRouteStore(raceId: 1, currentSceneId: 900));
        var finalService = CreateService(CreateRouteStore(raceId: 1, currentSceneId: 6014));

        var openingRoute = await openingService.GetPlayerRouteAsync(PlayerId);
        var finalRoute = await finalService.GetPlayerRouteAsync(PlayerId);

        AssertCurrentStep(openingRoute, order: 1, locationId: HerosOverlookLocationId);
        AssertCurrentStep(finalRoute, order: 9, locationId: HerosOverlookLocationId);
    }

    [Theory]
    [InlineData(6102, 8, DarkstormKeepLocationId)]
    [InlineData(6104, 7, TheBonePeaksLocationId)]
    public async Task GetPlayerRoute_EndingScenesOutsideHerosOverlook_AreNotFinalReturn(
        int currentSceneId,
        int expectedOrder,
        int expectedLocationId)
    {
        var service = CreateService(CreateRouteStore(raceId: 4, currentSceneId: currentSceneId));

        var route = await service.GetPlayerRouteAsync(PlayerId);

        AssertCurrentStep(route, expectedOrder, expectedLocationId);
        Assert.DoesNotContain(route, step => step.Order == 9 && step.IsCurrent);
    }

    [Fact]
    public async Task GetPlayerRoute_CurrentStep_IsDerivedFromCurrentScenarioScene()
    {
        var service = CreateService(CreateRouteStore(raceId: 1, currentSceneId: 3006));

        var route = await service.GetPlayerRouteAsync(PlayerId);

        var current = Assert.Single(route, step => step.IsCurrent);
        Assert.Equal(4, current.Order);
        Assert.Equal(OakheavenLocationId, current.LocationId);
        Assert.Equal("current", current.Status);
        Assert.True(route.Where(step => step.Order < current.Order).All(step => step.IsCompleted));
        Assert.True(route.Where(step => step.Order > current.Order).All(step => !step.IsCompleted));
        Assert.All(route.Where(step => step.Order < current.Order), step => Assert.Equal("completed", step.Status));
        Assert.All(route.Where(step => step.Order > current.Order), step => Assert.Equal("upcoming", step.Status));
    }

    [Fact]
    public async Task GetPlayerRoute_InvalidPlayer_FailsWithNotFound()
    {
        var service = CreateService(MockDataBootstrapper.CreateSeededStore());

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetPlayerRouteAsync(999));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task GetPlayerLocationStatuses_ValidPlayer_ReturnsCatalogLocations()
    {
        var service = CreateService(CreateRouteStore(raceId: 1, currentSceneId: 1002));

        var statuses = await service.GetPlayerLocationStatusesAsync(PlayerId);

        Assert.Equal(7, statuses.Count);
        Assert.All(statuses, status =>
        {
            Assert.NotEqual(0, status.LocationId);
            Assert.False(string.IsNullOrWhiteSpace(status.LocationName));
            Assert.False(string.IsNullOrWhiteSpace(status.Status));
            Assert.True(status.RecommendedLevel > 0);
        });
    }

    [Fact]
    public async Task GetPlayerLocationStatuses_CurrentLocation_IsDerivedFromCurrentScene()
    {
        var service = CreateService(CreateRouteStore(raceId: 1, currentSceneId: 1002));

        var statuses = await service.GetPlayerLocationStatusesAsync(PlayerId);

        var current = Assert.Single(statuses, status => status.IsCurrent);
        Assert.Equal(HerosOverlookLocationId, current.LocationId);
        Assert.Equal("current", current.Status);
    }

    [Fact]
    public async Task GetPlayerLocationStatuses_ReachableLocations_ComeFromEngineApprovedChoices()
    {
        var service = CreateService(CreateRouteStore(raceId: 1, currentSceneId: 4100));

        var statuses = await service.GetPlayerLocationStatusesAsync(PlayerId);

        var reachable = FindStatus(statuses, AshtoniaLocationId);
        Assert.False(reachable.IsCurrent);
        Assert.False(reachable.IsCompleted);
        Assert.Equal("reachable", reachable.Status);
    }

    [Fact]
    public async Task GetPlayerLocationStatuses_RepeatedRouteLocation_CompletesOnlyAfterAllRouteOccurrences()
    {
        var postRaceService = CreateService(CreateRouteStore(raceId: 1, currentSceneId: 2003));
        var oakheavenService = CreateService(CreateRouteStore(raceId: 1, currentSceneId: 3006));
        var repeatedFutureService = CreateService(CreateRouteStore(raceId: 3, currentSceneId: 2003));

        var postRaceStatuses = await postRaceService.GetPlayerLocationStatusesAsync(PlayerId);
        var oakheavenStatuses = await oakheavenService.GetPlayerLocationStatusesAsync(PlayerId);
        var repeatedFutureStatuses = await repeatedFutureService.GetPlayerLocationStatusesAsync(PlayerId);

        Assert.False(FindStatus(postRaceStatuses, MisthavenPortLocationId).IsCompleted);
        Assert.True(FindStatus(oakheavenStatuses, MisthavenPortLocationId).IsCompleted);
        Assert.False(FindStatus(repeatedFutureStatuses, AshtoniaLocationId).IsCompleted);
    }

    [Fact]
    public async Task GetPlayerLocationStatuses_OpeningScene_DoesNotInventCompletedOrUnlockedLocations()
    {
        var service = CreateService(CreateRouteStore(raceId: 1, currentSceneId: 1002));

        var statuses = await service.GetPlayerLocationStatusesAsync(PlayerId);

        Assert.DoesNotContain(statuses, status => status.IsCompleted);
        Assert.Equal("current", FindStatus(statuses, HerosOverlookLocationId).Status);
        Assert.Equal("reachable", FindStatus(statuses, MisthavenPortLocationId).Status);
        Assert.Equal("upcoming", FindStatus(statuses, OakheavenLocationId).Status);
        Assert.Equal("upcoming", FindStatus(statuses, DarkstormKeepLocationId).Status);
    }

    [Fact]
    public async Task GetPlayerLocationStatuses_InvalidPlayer_FailsWithNotFound()
    {
        var service = CreateService(MockDataBootstrapper.CreateSeededStore());

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetPlayerLocationStatusesAsync(999));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
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
        var characterRepository = new MockCharacterRepository(store);
        var gameSessionRepository = new MockGameSessionRepository(store);
        var scenarioProgressRepository = new MockScenarioProgressRepository(store);
        var scenarioService = new ScenarioService(
            new ScenarioEngine(),
            characterRepository,
            gameSessionRepository,
            scenarioProgressRepository,
            storySceneRepository,
            new MockQuestRepository(store),
            new MockPlayerQuestRepository(store),
            locationRepository);

        return new LocationProgressionService(
            locationRepository,
            scenarioService,
            storySceneRepository,
            characterRepository,
            gameSessionRepository,
            scenarioProgressRepository,
            new MockEnemyRepository(store));
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

    private static InMemoryGameDataStore CreateRouteStore(int raceId, int currentSceneId = 900)
    {
        var store = MockDataBootstrapper.CreateSeededStore();

        store.Characters.Add(new PlayerCharacter
        {
            Id = CharacterId,
            OwnerId = PlayerId.ToString(),
            Name = "Route Tester",
            RaceId = raceId,
            ClassId = 1,
            Level = 10
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
            CurrentSceneId = currentSceneId
        });

        return store;
    }

    private static void AssertRoute(IReadOnlyList<LocationRouteStepDto> route, IReadOnlyList<int> expectedLocationIds)
    {
        Assert.Equal(expectedLocationIds.Count, route.Count);
        Assert.Equal(Enumerable.Range(1, expectedLocationIds.Count), route.Select(step => step.Order));
        Assert.Equal(expectedLocationIds, route.Select(step => step.LocationId).ToList());
        Assert.Equal(ResolveLocationNames(expectedLocationIds), route.Select(step => step.LocationName).ToList());
        Assert.All(route, step => Assert.True(step.RecommendedLevel > 0));
    }

    private static void AssertCurrentStep(IReadOnlyList<LocationRouteStepDto> route, int order, int locationId)
    {
        var current = Assert.Single(route, step => step.IsCurrent);
        Assert.Equal(order, current.Order);
        Assert.Equal(locationId, current.LocationId);
        Assert.Equal("current", current.Status);
    }

    private static void AssertRouteProgress(
        IReadOnlyList<LocationRouteStepDto> route,
        int? currentOrder,
        int completedThroughOrder)
    {
        if (currentOrder is int order)
        {
            var current = Assert.Single(route, step => step.IsCurrent);
            Assert.Equal(order, current.Order);
            Assert.Equal("current", current.Status);
            Assert.False(current.IsCompleted);
        }
        else
        {
            Assert.DoesNotContain(route, step => step.IsCurrent);
        }

        foreach (var step in route)
        {
            if (step.Order <= completedThroughOrder)
            {
                Assert.True(step.IsCompleted);
                Assert.Equal("completed", step.Status);
            }
            else if (currentOrder == step.Order)
            {
                Assert.False(step.IsCompleted);
                Assert.Equal("current", step.Status);
            }
            else
            {
                Assert.False(step.IsCompleted);
                Assert.Equal("upcoming", step.Status);
            }
        }
    }

    private static LocationStatusDto FindStatus(IReadOnlyList<LocationStatusDto> statuses, int locationId)
    {
        return Assert.Single(statuses, status => status.LocationId == locationId);
    }

    private static void AssertEnemyIds(IReadOnlyList<LocationEnemyDto> enemies, params int[] expectedIds)
    {
        Assert.Equal(expectedIds, enemies.Select(enemy => enemy.Id).ToArray());
        Assert.All(enemies, enemy => Assert.False(string.IsNullOrWhiteSpace(enemy.Name)));
    }

    private static IReadOnlyList<string> ResolveLocationNames(IReadOnlyList<int> locationIds)
    {
        return locationIds.Select(id => id switch
        {
            AshtoniaLocationId => "Ashtonia",
            DarkstormKeepLocationId => "Darkstorm Keep",
            HerosOverlookLocationId => "Hero's Overlook",
            MisthavenPortLocationId => "Misthaven Port",
            OakheavenLocationId => "Oakheaven",
            TheBonePeaksLocationId => "The Bone Peaks",
            WhisperingWoodsLocationId => "Whispering Woods",
            _ => throw new InvalidOperationException($"Unexpected location {id}.")
        }).ToList();
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
