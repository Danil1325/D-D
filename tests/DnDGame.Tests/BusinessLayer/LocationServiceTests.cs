using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Locations;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Engine.Locations;
using DnDGame.Domain.Engine.Scenario;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using Xunit;

namespace DnDGame.Tests.BusinessLayer;

public class LocationServiceTests
{
    private const int CurrentPlayerId = 1;
    private const int TravelStartSceneId = 700001;
    private const int TravelMisthavenSceneId = 700002;
    private const int TravelWhisperingSceneId = 700003;
    private const int TravelOakheavenSceneId = 700004;
    private const int TravelSecondOakheavenSceneId = 700005;
    private const int TravelChoiceId = 710001;
    private const int TravelSecondChoiceId = 710002;

    // --- Catalogue reads ---

    [Fact]
    public async Task GetAllLocationsAsync_ReturnsFrontendSummaryShape()
    {
        var (service, _, _) = CreateScenario();

        var locations = await service.GetAllLocationsAsync();

        Assert.Equal(7, locations.Count);
        var herosOverlook = locations.Single(location => location.Id == (int)LocationId.HerosOverlook);
        Assert.Equal("heros-overlook", herosOverlook.Slug);
        Assert.Equal("Hero's Overlook", herosOverlook.Name);
        Assert.Equal(1, herosOverlook.RecommendedMinimumLevel);
        Assert.False(string.IsNullOrWhiteSpace(herosOverlook.BackgroundImage));
        Assert.True(herosOverlook.IsSafeLocation);
    }

    [Fact]
    public async Task GetLocationByIdAsync_ReturnsFrontendDetailsShape()
    {
        var (service, _, _) = CreateScenario();

        var details = await service.GetLocationByIdAsync(LocationId.HerosOverlook);

        Assert.Equal((int)LocationId.HerosOverlook, details.Id);
        Assert.Equal("heros-overlook", details.Slug);
        Assert.Equal("Hero's Overlook", details.Name);
        Assert.False(string.IsNullOrWhiteSpace(details.Description));
        Assert.Equal(1, details.RecommendedMinimumLevel);
        Assert.False(string.IsNullOrWhiteSpace(details.BackgroundImage));
        Assert.True(details.IsSafeLocation);
    }

    [Fact]
    public async Task GetLocationByIdAsync_DoesNotRequireCurrentPlayerProgress()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var service = CreateService(store);

        var details = await service.GetLocationByIdAsync(LocationId.HerosOverlook);

        Assert.Equal((int)LocationId.HerosOverlook, details.Id);
    }

    [Fact]
    public async Task GetLocationByIdAsync_InvalidLocation_FailsWithValidationError()
    {
        var (service, _, _) = CreateScenario();

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.GetLocationByIdAsync((LocationId)999));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
    }

    // --- Player-scoped progress / route ---

    [Fact]
    public async Task GetProgressForPlayerAsync_ReturnsSnapshotForTheGivenPlayer()
    {
        var (service, store, player) = CreateScenario();
        store.LocationProgresses.Add(new LocationProgress
        {
            PlayerId = player.Id,
            LocationId = LocationId.HerosOverlook,
            Status = LocationStatus.Current,
            Visited = true
        });

        var progress = await service.GetProgressForPlayerAsync(CurrentPlayerId);

        Assert.Equal(7, progress.Count);
        var herosOverlook = progress.Single(location => location.LocationId == (int)LocationId.HerosOverlook);
        Assert.Equal("Hero's Overlook", herosOverlook.LocationName);
        Assert.Equal("Current", herosOverlook.Status);
        Assert.Equal(1, herosOverlook.RecommendedLevel);
        Assert.True(herosOverlook.IsCurrent);
        Assert.False(herosOverlook.IsCompleted);
    }

    [Fact]
    public async Task GetProgressForPlayerAsync_ThrowsNotFound_WhenNoCharacterOwnedByPlayer()
    {
        var (service, _, _) = CreateScenario();

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.GetProgressForPlayerAsync(999));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Theory]
    [InlineData((int)RaceType.Human, new[] { LocationId.HerosOverlook, LocationId.MisthavenPort, LocationId.MisthavenPort, LocationId.Oakheaven, LocationId.Ashtonia, LocationId.WhisperingWoods, LocationId.TheBonePeaks, LocationId.DarkstormKeep, LocationId.HerosOverlook })]
    [InlineData((int)RaceType.Elf, new[] { LocationId.HerosOverlook, LocationId.WhisperingWoods, LocationId.MisthavenPort, LocationId.Oakheaven, LocationId.Ashtonia, LocationId.WhisperingWoods, LocationId.TheBonePeaks, LocationId.DarkstormKeep, LocationId.HerosOverlook })]
    [InlineData((int)RaceType.Orc, new[] { LocationId.HerosOverlook, LocationId.Ashtonia, LocationId.MisthavenPort, LocationId.Oakheaven, LocationId.Ashtonia, LocationId.WhisperingWoods, LocationId.TheBonePeaks, LocationId.DarkstormKeep, LocationId.HerosOverlook })]
    [InlineData((int)RaceType.Dwarf, new[] { LocationId.HerosOverlook, LocationId.TheBonePeaks, LocationId.MisthavenPort, LocationId.Oakheaven, LocationId.Ashtonia, LocationId.WhisperingWoods, LocationId.TheBonePeaks, LocationId.DarkstormKeep, LocationId.HerosOverlook })]
    public async Task GetRouteForPlayerAsync_ReturnsAuthoredRaceRoute(int raceId, LocationId[] expectedLocations)
    {
        var (service, _, _) = CreateScenario(raceId: raceId);

        var route = await service.GetRouteForPlayerAsync(CurrentPlayerId);

        AssertRoute(route, expectedLocations);
        Assert.All(route, step =>
        {
            Assert.False(string.IsNullOrWhiteSpace(step.LocationName));
            Assert.False(string.IsNullOrWhiteSpace(step.Status));
            Assert.True(step.RecommendedLevel > 0);
        });
    }

    [Fact]
    public async Task GetRouteForPlayerAsync_HumanRaceSpecificMisthaven_IsOrderTwo()
    {
        var (service, store, _) = CreateScenario(raceId: (int)RaceType.Human);
        SetCurrentScene(store, 1203);

        var route = await service.GetRouteForPlayerAsync(CurrentPlayerId);

        AssertCurrentStep(route, order: 2, locationId: LocationId.MisthavenPort);
        Assert.False(route.Single(step => step.Order == 3).IsCurrent);
    }

    [Fact]
    public async Task GetRouteForPlayerAsync_HumanPostConvergenceMisthaven_IsOrderThree()
    {
        var (service, store, _) = CreateScenario(raceId: (int)RaceType.Human);
        SetCurrentScene(store, 2003);

        var route = await service.GetRouteForPlayerAsync(CurrentPlayerId);

        AssertCurrentStep(route, order: 3, locationId: LocationId.MisthavenPort);
        Assert.True(route.Single(step => step.Order == 2).IsCompleted);
    }

    [Fact]
    public async Task GetRouteForPlayerAsync_RaceBranchInterstitial_HasNoCurrentStep()
    {
        var (service, store, _) = CreateScenario(raceId: (int)RaceType.Human);
        SetCurrentScene(store, 1205);

        var route = await service.GetRouteForPlayerAsync(CurrentPlayerId);

        AssertRouteProgress(route, currentOrder: null, completedThroughOrder: 1);
        Assert.False(route.Single(step => step.Order == 2).IsCompleted);
        Assert.False(route.Single(step => step.Order == 3).IsCompleted);
    }

    [Fact]
    public async Task GetRouteForPlayerAsync_ActFourGatheringInterstitial_DoesNotRegressBelowOakheaven()
    {
        var (service, store, _) = CreateScenario(raceId: (int)RaceType.Human);
        SetCurrentScene(store, 4100);

        var route = await service.GetRouteForPlayerAsync(CurrentPlayerId);

        AssertRouteProgress(route, currentOrder: null, completedThroughOrder: 4);
        Assert.False(route.Single(step => step.Order == 5).IsCompleted);
    }

    [Theory]
    [InlineData((int)RaceType.Elf, 4009, 6, LocationId.WhisperingWoods)]
    [InlineData((int)RaceType.Orc, 4008, 5, LocationId.Ashtonia)]
    [InlineData((int)RaceType.Dwarf, 4010, 7, LocationId.TheBonePeaks)]
    public async Task GetRouteForPlayerAsync_RepeatedRaceLocationFragmentScenes_ResolveToFragmentStep(
        int raceId,
        int currentSceneId,
        int expectedOrder,
        LocationId expectedLocationId)
    {
        var (service, store, _) = CreateScenario(raceId: raceId);
        SetCurrentScene(store, currentSceneId);

        var route = await service.GetRouteForPlayerAsync(CurrentPlayerId);

        AssertCurrentStep(route, expectedOrder, expectedLocationId);
    }

    [Fact]
    public async Task GetRouteForPlayerAsync_OpeningAndFinalHerosOverlook_AreDistinctSteps()
    {
        var (openingService, openingStore, _) = CreateScenario(raceId: (int)RaceType.Human);
        SetCurrentScene(openingStore, 1002);
        var (finalService, finalStore, _) = CreateScenario(raceId: (int)RaceType.Human);
        SetCurrentScene(finalStore, 6014);

        var openingRoute = await openingService.GetRouteForPlayerAsync(CurrentPlayerId);
        var finalRoute = await finalService.GetRouteForPlayerAsync(CurrentPlayerId);

        AssertCurrentStep(openingRoute, order: 1, locationId: LocationId.HerosOverlook);
        AssertCurrentStep(finalRoute, order: 9, locationId: LocationId.HerosOverlook);
    }

    [Theory]
    [InlineData(6102, 8, LocationId.DarkstormKeep)]
    [InlineData(6104, 7, LocationId.TheBonePeaks)]
    public async Task GetRouteForPlayerAsync_EndingScenesOutsideHerosOverlook_AreNotFinalReturn(
        int currentSceneId,
        int expectedOrder,
        LocationId expectedLocationId)
    {
        var (service, store, _) = CreateScenario(raceId: (int)RaceType.Dwarf);
        SetCurrentScene(store, currentSceneId);

        var route = await service.GetRouteForPlayerAsync(CurrentPlayerId);

        AssertCurrentStep(route, expectedOrder, expectedLocationId);
        Assert.False(route.Single(step => step.Order == 9).IsCurrent);
    }

    [Fact]
    public async Task GetRouteForPlayerAsync_AuthoredProgression_IsMonotonicThroughInterstitials()
    {
        var progression = new (int SceneId, int? CurrentOrder, int CompletedThroughOrder)[]
        {
            (1002, 1, 0),
            (1205, null, 1),
            (1203, 2, 1),
            (2003, 3, 2),
            (3006, 4, 3),
            (4100, null, 4),
            (4008, 5, 4),
            (5101, 8, 7),
            (6014, 9, 8)
        };
        var previousRouteCursor = 0;

        foreach (var (sceneId, currentOrder, completedThroughOrder) in progression)
        {
            var (service, store, _) = CreateScenario(raceId: (int)RaceType.Human);
            SetCurrentScene(store, sceneId);

            var route = await service.GetRouteForPlayerAsync(CurrentPlayerId);

            AssertRouteProgress(route, currentOrder, completedThroughOrder);
            var routeCursor = currentOrder ?? completedThroughOrder;
            Assert.True(routeCursor >= previousRouteCursor);
            previousRouteCursor = routeCursor;
        }
    }

    // --- Enemies (delegates to ILocationEncounterService) ---

    [Fact]
    public async Task GetAvailableEnemiesAsync_ReturnsNoEnemies_BelowRecommendedMinimumLevel()
    {
        var (service, _, _) = CreateScenario(characterLevel: 1); // Darkstorm Keep requires level 8

        var enemies = await service.GetAvailableEnemiesAsync(LocationId.DarkstormKeep, CurrentPlayerId);

        Assert.Empty(enemies);
    }

    [Fact]
    public async Task GetAvailableEnemiesAsync_ReturnsEncounters_AtOrAboveRecommendedMinimumLevel()
    {
        var (service, _, _) = CreateScenario(characterLevel: 1); // Whispering Woods requires level 1

        var enemies = await service.GetAvailableEnemiesAsync(LocationId.WhisperingWoods, CurrentPlayerId);

        Assert.NotEmpty(enemies);
        Assert.All(enemies, enemy =>
        {
            Assert.True(enemy.Id > 0);
            Assert.False(string.IsNullOrWhiteSpace(enemy.Name));
        });
    }

    // --- Travel ---

    [Fact]
    public async Task TravelToLocationAsync_ValidReachableLocation_UsesScenarioChoiceAndReturnsCurrentLocationAndScene()
    {
        var (service, store, player) = CreateScenario();
        store.LocationProgresses.Add(new LocationProgress
        {
            PlayerId = player.Id,
            LocationId = LocationId.HerosOverlook,
            Status = LocationStatus.Current,
            Visited = true
        });
        SeedTravelScenario(
            store,
            Scene(TravelStartSceneId, LocationId.HerosOverlook,
                Choice(TravelChoiceId, TravelMisthavenSceneId, consequence: new ChoiceConsequence
                {
                    StoryFlags = new Dictionary<string, bool> { ["travel_via_engine"] = true }
                })),
            Scene(TravelMisthavenSceneId, LocationId.MisthavenPort));

        var result = await service.TravelToLocationAsync(
            new TravelToLocationRequest { PlayerId = CurrentPlayerId, LocationId = LocationId.MisthavenPort });

        Assert.Equal((int)LocationId.MisthavenPort, result.CurrentLocation.Id);
        Assert.Equal(TravelMisthavenSceneId, result.CurrentScene.Id);
        Assert.Equal((int)LocationId.MisthavenPort, result.CurrentScene.LocationId);

        var progress = store.ScenarioProgresses.Single();
        Assert.Equal(TravelMisthavenSceneId, progress.CurrentSceneId);
        Assert.True(progress.StoryFlags["travel_via_engine"]);

        var herosOverlook = store.LocationProgresses.Single(p => p.LocationId == LocationId.HerosOverlook);
        var misthavenPort = store.LocationProgresses.Single(p => p.LocationId == LocationId.MisthavenPort);
        Assert.Equal(LocationStatus.Available, herosOverlook.Status);
        Assert.Equal(LocationStatus.Current, misthavenPort.Status);
    }

    [Fact]
    public async Task TravelToLocationAsync_UnreachableLocation_IsRejectedAndLeavesScenarioProgressUnchanged()
    {
        var (service, store, _) = CreateScenario();
        SeedTravelScenario(
            store,
            Scene(TravelStartSceneId, LocationId.HerosOverlook,
                Choice(TravelChoiceId, TravelMisthavenSceneId)),
            Scene(TravelMisthavenSceneId, LocationId.MisthavenPort));

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.TravelToLocationAsync(
            new TravelToLocationRequest { PlayerId = CurrentPlayerId, LocationId = LocationId.DarkstormKeep }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
        Assert.Equal(TravelStartSceneId, store.ScenarioProgresses.Single().CurrentSceneId);
    }

    [Fact]
    public async Task TravelToLocationAsync_SameCurrentLocation_IsRejectedAndLeavesScenarioProgressUnchanged()
    {
        var (service, store, _) = CreateScenario();
        SeedTravelScenario(
            store,
            Scene(TravelStartSceneId, LocationId.HerosOverlook,
                Choice(TravelChoiceId, TravelMisthavenSceneId)),
            Scene(TravelMisthavenSceneId, LocationId.MisthavenPort));

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.TravelToLocationAsync(
            new TravelToLocationRequest { PlayerId = CurrentPlayerId, LocationId = LocationId.HerosOverlook }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
        Assert.Equal(TravelStartSceneId, store.ScenarioProgresses.Single().CurrentSceneId);
    }

    [Fact]
    public async Task TravelToLocationAsync_RaceGatedUnavailableChoice_IsRejected()
    {
        var (service, store, _) = CreateScenario(raceId: (int)RaceType.Human);
        SeedTravelScenario(
            store,
            Scene(TravelStartSceneId, LocationId.HerosOverlook,
                Choice(TravelChoiceId, TravelWhisperingSceneId, requirement: new ChoiceRequirement { RaceId = (int)RaceType.Elf })),
            Scene(TravelWhisperingSceneId, LocationId.WhisperingWoods));

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.TravelToLocationAsync(
            new TravelToLocationRequest { PlayerId = CurrentPlayerId, LocationId = LocationId.WhisperingWoods }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
        Assert.Equal(TravelStartSceneId, store.ScenarioProgresses.Single().CurrentSceneId);
    }

    [Fact]
    public async Task TravelToLocationAsync_AmbiguousLocationTransition_IsRejected()
    {
        var (service, store, _) = CreateScenario();
        SeedTravelScenario(
            store,
            Scene(TravelStartSceneId, LocationId.HerosOverlook,
                Choice(TravelChoiceId, TravelOakheavenSceneId),
                Choice(TravelSecondChoiceId, TravelSecondOakheavenSceneId)),
            Scene(TravelOakheavenSceneId, LocationId.Oakheaven),
            Scene(TravelSecondOakheavenSceneId, LocationId.Oakheaven));

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.TravelToLocationAsync(
            new TravelToLocationRequest { PlayerId = CurrentPlayerId, LocationId = LocationId.Oakheaven }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
        Assert.Equal(TravelStartSceneId, store.ScenarioProgresses.Single().CurrentSceneId);
    }

    [Fact]
    public async Task TravelToLocationAsync_ThrowsNotFound_WhenNoCharacterOwnedByPlayer()
    {
        var (service, _, _) = CreateScenario();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.TravelToLocationAsync(
            new TravelToLocationRequest { PlayerId = 999, LocationId = LocationId.HerosOverlook }));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    private static void SeedTravelScenario(InMemoryGameDataStore store, params StoryScene[] scenes)
    {
        store.StoryScenes.AddRange(scenes);
        store.ScenarioProgresses.Add(new ScenarioProgress
        {
            GameSessionId = 1,
            CurrentSceneId = TravelStartSceneId
        });
    }

    private static void SetCurrentScene(InMemoryGameDataStore store, int currentSceneId)
    {
        store.ScenarioProgresses.Clear();
        store.ScenarioProgresses.Add(new ScenarioProgress
        {
            GameSessionId = 1,
            CurrentSceneId = currentSceneId
        });
    }

    private static void AssertRoute(IReadOnlyList<LocationRouteDto> route, IReadOnlyList<LocationId> expectedLocations)
    {
        Assert.Equal(expectedLocations.Count, route.Count);
        Assert.Equal(Enumerable.Range(1, expectedLocations.Count), route.Select(step => step.Order));
        Assert.Equal(expectedLocations.Select(locationId => (int)locationId), route.Select(step => step.LocationId));
    }

    private static void AssertCurrentStep(IReadOnlyList<LocationRouteDto> route, int order, LocationId locationId)
    {
        var current = Assert.Single(route, step => step.IsCurrent);
        Assert.Equal(order, current.Order);
        Assert.Equal((int)locationId, current.LocationId);
        Assert.Equal(LocationStatus.Current.ToString(), current.Status);
        Assert.False(current.IsCompleted);
    }

    private static void AssertRouteProgress(
        IReadOnlyList<LocationRouteDto> route,
        int? currentOrder,
        int completedThroughOrder)
    {
        if (currentOrder is int order)
        {
            var current = Assert.Single(route, step => step.IsCurrent);
            Assert.Equal(order, current.Order);
            Assert.Equal(LocationStatus.Current.ToString(), current.Status);
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
                Assert.Equal(LocationStatus.Completed.ToString(), step.Status);
            }
            else if (currentOrder == step.Order)
            {
                Assert.False(step.IsCompleted);
                Assert.Equal(LocationStatus.Current.ToString(), step.Status);
            }
            else
            {
                Assert.False(step.IsCompleted);
                Assert.NotEqual(LocationStatus.Current.ToString(), step.Status);
                Assert.NotEqual(LocationStatus.Completed.ToString(), step.Status);
            }
        }
    }

    private static StoryScene Scene(int id, LocationId locationId, params StoryChoice[] choices)
    {
        return new StoryScene
        {
            Id = id,
            Act = 1,
            Chapter = 1,
            Title = $"Travel Scene {id}",
            LocationId = (int)locationId,
            BackgroundImage = locationId.ToString(),
            Choices = choices.ToList()
        };
    }

    private static StoryChoice Choice(
        int id,
        int nextSceneId,
        ChoiceRequirement? requirement = null,
        ChoiceConsequence? consequence = null)
    {
        var choice = new StoryChoice
        {
            Id = id,
            Text = $"Travel Choice {id}",
            NextSceneId = nextSceneId
        };

        if (requirement is not null)
        {
            choice.Requirements.Add(requirement);
        }

        if (consequence is not null)
        {
            choice.Consequences.Add(consequence);
        }

        return choice;
    }

    // --- Test scaffolding ---

    private static (ILocationService Service, InMemoryGameDataStore Store, PlayerCharacter Player) CreateScenario(
        int characterLevel = 1,
        int raceId = 1)
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var player = new PlayerCharacter
        {
            Id = CurrentPlayerId,
            OwnerId = CurrentPlayerId.ToString(),
            Name = "Hero",
            Level = characterLevel,
            MaxHealth = 20,
            CurrentHealth = 20,
            RaceId = raceId,
            ClassId = 1
        };
        store.Characters.Add(player);
        store.GameSessions.Add(new GameSession { Id = 1, CharacterId = CurrentPlayerId, AdventureId = 1 });

        return (CreateService(store), store, player);
    }

    private static ILocationService CreateService(InMemoryGameDataStore store)
    {
        var locationEncounterService = new LocationEncounterService(
            new MockLocationEncounterRepository(store),
            new MockLocationDefinitionRepository(store),
            new MockEnemyRepository(store),
            new MockGameSessionRepository(store),
            new MockCharacterRepository(store),
            new MockScenarioProgressRepository(store),
            new MockPlayerQuestRepository(store),
            new MockBattleRepository(store),
            new FixedCurrentPlayerService(CurrentPlayerId));

        var scenarioService = new ScenarioService(
            new ScenarioEngine(),
            new MockCharacterRepository(store),
            new MockGameSessionRepository(store),
            new MockScenarioProgressRepository(store),
            new MockStorySceneRepository(store),
            new MockQuestRepository(store),
            new MockPlayerQuestRepository(store),
            new MockLocationRepository(store),
            new ExplicitLocationUnlockService(
                new MockLocationDefinitionRepository(store),
                new MockLocationProgressRepository(store),
                new AchievementService(
                    new MockAchievementRepository(store),
                    new MockAchievementProgressRepository(store),
                    new MockAchievementEventRepository(store),
                    new MockCharacterRepository(store),
                    new FixedCurrentPlayerService(CurrentPlayerId))));

        return new LocationService(
            new MockLocationDefinitionRepository(store),
            new MockLocationProgressRepository(store),
            new MockCharacterRepository(store),
            new MockGameSessionRepository(store),
            new MockPlayerQuestRepository(store),
            new MockScenarioProgressRepository(store),
            locationEncounterService,
            new LocationUnlockEngine(),
            new LocationRouteProvider(),
            new FixedCurrentPlayerService(CurrentPlayerId),
            scenarioService,
            new MockStorySceneRepository(store));
    }

    private sealed class FixedCurrentPlayerService : ICurrentPlayerService
    {
        private readonly int _playerId;

        public FixedCurrentPlayerService(int playerId)
        {
            _playerId = playerId;
        }

        public int GetCurrentPlayerId() => _playerId;
    }
}
