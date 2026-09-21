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

    [Fact]
    public async Task GetRouteForPlayerAsync_ReturnsTheCharactersRaceRoute_StartingAtHerosOverlook()
    {
        var (service, _, _) = CreateScenario(raceId: (int)RaceType.Dwarf);

        var route = await service.GetRouteForPlayerAsync(CurrentPlayerId);

        Assert.Equal((int)LocationId.HerosOverlook, route.OrderBy(step => step.Order).First().LocationId);
        Assert.Contains(route, step => step.LocationId == (int)LocationId.TheBonePeaks);
        Assert.All(route, step =>
        {
            Assert.True(step.Order > 0);
            Assert.False(string.IsNullOrWhiteSpace(step.LocationName));
            Assert.False(string.IsNullOrWhiteSpace(step.Status));
            Assert.True(step.RecommendedLevel > 0);
        });
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
                new MockLocationProgressRepository(store)));

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
