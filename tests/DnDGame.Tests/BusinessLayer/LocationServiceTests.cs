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
