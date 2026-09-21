using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Locations;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;

namespace DnDGame.Tests.BusinessLayer;

public class ExplicitLocationUnlockServiceTests
{
    [Fact]
    public async Task UnlockExplicitLocations_DuplicateRequestedIds_ReturnsLocationOnce()
    {
        var (service, store, character) = CreateScenario();

        var result = await service.UnlockExplicitLocationsAsync(character, new[] { 4, 4 });

        Assert.Equal(new[] { LocationId.MisthavenPort }, result);
        var progress = Assert.Single(store.LocationProgresses);
        Assert.Equal(character.Id, progress.PlayerId);
        Assert.Equal(LocationId.MisthavenPort, progress.LocationId);
        Assert.Equal(LocationStatus.Available, progress.Status);
    }

    [Fact]
    public async Task UnlockExplicitLocations_AlreadyUnlockedLocation_IsNotReturnedAgain()
    {
        var (service, store, character) = CreateScenario();
        store.LocationProgresses.Add(new LocationProgress
        {
            PlayerId = character.Id,
            LocationId = LocationId.MisthavenPort,
            Status = LocationStatus.Available,
            UnlockedAtLevel = 1
        });

        var result = await service.UnlockExplicitLocationsAsync(character, new[] { 4 });

        Assert.Empty(result);
        var progress = Assert.Single(store.LocationProgresses);
        Assert.Equal(LocationStatus.Available, progress.Status);
        Assert.Equal(1, progress.UnlockedAtLevel);
    }

    [Fact]
    public async Task UnlockExplicitLocations_MixedExistingAndNew_ReturnsOnlyNewLocations()
    {
        var (service, store, character) = CreateScenario();
        store.LocationProgresses.Add(new LocationProgress
        {
            PlayerId = character.Id,
            LocationId = LocationId.MisthavenPort,
            Status = LocationStatus.Current,
            UnlockedAtLevel = 1
        });

        var result = await service.UnlockExplicitLocationsAsync(character, new[] { 4, 5 });

        Assert.Equal(new[] { LocationId.Oakheaven }, result);
        Assert.Contains(store.LocationProgresses, progress =>
            progress.LocationId == LocationId.MisthavenPort &&
            progress.Status == LocationStatus.Current);
        Assert.Contains(store.LocationProgresses, progress =>
            progress.LocationId == LocationId.Oakheaven &&
            progress.Status == LocationStatus.Available);
    }

    [Fact]
    public async Task UnlockExplicitLocations_EmptyInput_ReturnsEmpty()
    {
        var (service, store, character) = CreateScenario();

        var result = await service.UnlockExplicitLocationsAsync(character, Array.Empty<int>());

        Assert.Empty(result);
        Assert.Empty(store.LocationProgresses);
    }

    [Fact]
    public async Task UnlockExplicitLocations_InvalidLocationId_FailsWithValidationError()
    {
        var (service, store, character) = CreateScenario();

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            service.UnlockExplicitLocationsAsync(character, new[] { 999 }));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
        Assert.Empty(store.LocationProgresses);
    }

    [Fact]
    public async Task UnlockExplicitLocations_LockedProgressRow_BecomesAvailableAndIsReturned()
    {
        var (service, store, character) = CreateScenario();
        store.LocationProgresses.Add(new LocationProgress
        {
            PlayerId = character.Id,
            LocationId = LocationId.MisthavenPort,
            Status = LocationStatus.Locked
        });

        var result = await service.UnlockExplicitLocationsAsync(character, new[] { 4 });

        Assert.Equal(new[] { LocationId.MisthavenPort }, result);
        var progress = Assert.Single(store.LocationProgresses);
        Assert.Equal(LocationStatus.Available, progress.Status);
        Assert.Equal(character.Level, progress.UnlockedAtLevel);
    }

    private static (IExplicitLocationUnlockService Service, InMemoryGameDataStore Store, PlayerCharacter Character) CreateScenario()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var character = new PlayerCharacter
        {
            Id = 1,
            OwnerId = "1",
            Name = "Hero",
            Level = 5,
            RaceId = 1,
            ClassId = 1
        };

        store.Characters.Add(character);

        return (
            new ExplicitLocationUnlockService(
                new MockLocationDefinitionRepository(store),
                new MockLocationProgressRepository(store)),
            store,
            character);
    }
}
