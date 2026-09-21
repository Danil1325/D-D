using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Locations;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Locations;
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

    // --- Catalogue reads ---

    [Fact]
    public async Task GetAllLocationsAsync_ReturnsAllSevenLocations_LockedByDefault()
    {
        var (service, _, _) = CreateScenario();

        var locations = await service.GetAllLocationsAsync();

        Assert.Equal(7, locations.Count);
        Assert.All(locations, location => Assert.Equal(LocationStatus.Locked, location.Status));
        Assert.All(locations, location => Assert.False(location.IsCurrent));
    }

    [Fact]
    public async Task GetAllLocationsAsync_RecommendsHerosOverlookForAFreshCharacter()
    {
        var (service, _, _) = CreateScenario();

        var locations = await service.GetAllLocationsAsync();

        var herosOverlook = locations.Single(location => location.Id == LocationId.HerosOverlook);
        Assert.True(herosOverlook.IsRecommendedNext);
    }

    [Fact]
    public async Task GetLocationByIdAsync_ReflectsPersistedProgress()
    {
        var (service, store, player) = CreateScenario();
        store.LocationProgresses.Add(new LocationProgress
        {
            PlayerId = player.Id,
            LocationId = LocationId.HerosOverlook,
            Status = LocationStatus.Current,
            Visited = true,
            UnlockedAtLevel = 1
        });

        var details = await service.GetLocationByIdAsync(LocationId.HerosOverlook);

        Assert.Equal(LocationStatus.Current, details.Status);
        Assert.True(details.IsCurrent);
        Assert.True(details.Visited);
    }

    [Fact]
    public async Task GetLocationByIdAsync_ThrowsNotFound_ForCurrentPlayerWithNoCharacter()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var service = CreateService(store);

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.GetLocationByIdAsync(LocationId.HerosOverlook));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
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

        Assert.Equal(player.Id, progress.PlayerId);
        Assert.Equal(LocationId.HerosOverlook, progress.CurrentLocationId);
        Assert.Equal(7, progress.Locations.Count);
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

        Assert.Equal(RaceType.Dwarf, route.Race);
        Assert.Equal(LocationId.HerosOverlook, route.Steps.OrderBy(step => step.Order).First().LocationId);
        Assert.Contains(route.Steps, step => step.LocationId == LocationId.TheBonePeaks && step.RouteSegment == "Exterior");
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
        Assert.Contains(enemies, e => e.Tier == EncounterTier.Boss);
    }

    // --- Travel ---

    [Fact]
    public async Task TravelToLocationAsync_FirstTravel_UnlocksHerosOverlookAndSetsItCurrent()
    {
        var (service, store, player) = CreateScenario();

        var result = await service.TravelToLocationAsync(
            new TravelToLocationRequest { PlayerId = CurrentPlayerId, LocationId = LocationId.HerosOverlook });

        Assert.Equal(LocationStatus.Current, result.Status);
        Assert.Equal("Hero's Overlook", result.LocationName);

        var persisted = store.LocationProgresses.Single(p => p.PlayerId == player.Id && p.LocationId == LocationId.HerosOverlook);
        Assert.Equal(LocationStatus.Current, persisted.Status);
        Assert.True(persisted.Visited);
    }

    [Fact]
    public async Task TravelToLocationAsync_RejectsALocationWhoseRequirementsAreNotMet()
    {
        var (service, _, _) = CreateScenario(characterLevel: 1);

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.TravelToLocationAsync(
            new TravelToLocationRequest { PlayerId = CurrentPlayerId, LocationId = LocationId.DarkstormKeep }));

        Assert.Equal(EngineErrorCodes.LocationRequirementNotMet, exception.ErrorCode);
    }

    [Fact]
    public async Task TravelToLocationAsync_RevisitingAnAlreadyUnlockedLocation_JustMovesCurrent()
    {
        var (service, store, player) = CreateScenario();
        store.LocationProgresses.Add(new LocationProgress
        {
            PlayerId = player.Id,
            LocationId = LocationId.HerosOverlook,
            Status = LocationStatus.Available
        });

        var result = await service.TravelToLocationAsync(
            new TravelToLocationRequest { PlayerId = CurrentPlayerId, LocationId = LocationId.HerosOverlook });

        Assert.Equal(LocationStatus.Current, result.Status);
        var persisted = store.LocationProgresses.Single(p => p.PlayerId == player.Id && p.LocationId == LocationId.HerosOverlook);
        Assert.Equal(LocationStatus.Current, persisted.Status);
    }

    [Fact]
    public async Task TravelToLocationAsync_MovingToANewCurrentLocation_DemotesThePreviousOneToAvailable()
    {
        var (service, store, player) = CreateScenario();
        store.LocationProgresses.Add(new LocationProgress
        {
            PlayerId = player.Id,
            LocationId = LocationId.HerosOverlook,
            Status = LocationStatus.Current,
            Visited = true
        });
        store.LocationProgresses.Add(new LocationProgress
        {
            PlayerId = player.Id,
            LocationId = LocationId.MisthavenPort,
            Status = LocationStatus.Available
        });

        await service.TravelToLocationAsync(
            new TravelToLocationRequest { PlayerId = CurrentPlayerId, LocationId = LocationId.MisthavenPort });

        var herosOverlook = store.LocationProgresses.Single(p => p.LocationId == LocationId.HerosOverlook);
        var misthavenPort = store.LocationProgresses.Single(p => p.LocationId == LocationId.MisthavenPort);
        Assert.Equal(LocationStatus.Available, herosOverlook.Status);
        Assert.Equal(LocationStatus.Current, misthavenPort.Status);
    }

    [Fact]
    public async Task TravelToLocationAsync_ThrowsNotFound_WhenNoCharacterOwnedByPlayer()
    {
        var (service, _, _) = CreateScenario();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.TravelToLocationAsync(
            new TravelToLocationRequest { PlayerId = 999, LocationId = LocationId.HerosOverlook }));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
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
            new FixedCurrentPlayerService(CurrentPlayerId));
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
