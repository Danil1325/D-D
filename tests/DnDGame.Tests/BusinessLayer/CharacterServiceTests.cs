using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Characters;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using DnDGame.MockData.Services;

namespace DnDGame.Tests.BusinessLayer;

public class CharacterServiceTests
{
    [Fact]
    public async Task CreateNewGameAsync_ValidRequest_CreatesCharacterWithRolledAttributes()
    {
        var (service, _) = CreateService();

        var result = await service.CreateNewGameAsync(new NewGameCharacterRequestDto
        {
            Name = "Grommash",
            Race = RaceType.Orc,
            ClassId = 2
        });

        Assert.True(result.Id > 0);
        Assert.Equal("Grommash", result.Name);
        Assert.Equal("Orc", result.RaceName);
        Assert.Equal("Warrior", result.ClassName);
        Assert.Equal(1, result.Level);
        Assert.Equal(result.MaxHealth, result.CurrentHealth);
        Assert.InRange(result.Strength, 13, 18);
        Assert.InRange(result.Dexterity, 7, 11);
        Assert.InRange(result.Intelligence, 5, 9);
        Assert.InRange(result.Charisma, 6, 10);
        Assert.InRange(result.MaxHealth, 16, 22);
        Assert.False(string.IsNullOrEmpty(result.PortraitPath));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public async Task CreateNewGameAsync_InvalidName_ThrowsValidationError(string name)
    {
        var (service, _) = CreateService();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.CreateNewGameAsync(new NewGameCharacterRequestDto
        {
            Name = name,
            Race = RaceType.Human,
            ClassId = 1
        }));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateNewGameAsync_InvalidRace_ThrowsValidationError()
    {
        var (service, _) = CreateService();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.CreateNewGameAsync(new NewGameCharacterRequestDto
        {
            Name = "Grommash",
            Race = (RaceType)99,
            ClassId = 2
        }));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateNewGameAsync_UnknownClass_ThrowsClassNotFound()
    {
        var (service, _) = CreateService();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.CreateNewGameAsync(new NewGameCharacterRequestDto
        {
            Name = "Grommash",
            Race = RaceType.Orc,
            ClassId = 999
        }));

        Assert.Equal(CharacterErrorCodes.ClassNotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateNewGameAsync_SecondCharacterForSameOwner_ThrowsConflict()
    {
        var (service, _) = CreateService();
        await service.CreateNewGameAsync(new NewGameCharacterRequestDto
        {
            Name = "Grommash",
            Race = RaceType.Orc,
            ClassId = 2
        });

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.CreateNewGameAsync(new NewGameCharacterRequestDto
        {
            Name = "Grommash",
            Race = RaceType.Orc,
            ClassId = 2
        }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateNewGameAsync_SetsOwnerToTheCurrentPlayer()
    {
        var (service, store) = CreateService();

        var result = await service.CreateNewGameAsync(new NewGameCharacterRequestDto
        {
            Name = "Grommash",
            Race = RaceType.Orc,
            ClassId = 2
        });

        Assert.Equal(MockCurrentPlayerService.MockPlayerId, result.PlayerId);
        var character = Assert.Single(store.Characters);
        Assert.Equal("1", character.OwnerId);
    }

    [Fact]
    public async Task GetCurrentAsync_AfterCreation_ReturnsTheCreatedCharacter()
    {
        var (service, _) = CreateService();
        var created = await service.CreateNewGameAsync(new NewGameCharacterRequestDto
        {
            Name = "Grommash",
            Race = RaceType.Orc,
            ClassId = 2
        });

        var current = await service.GetCurrentAsync();

        Assert.Equal(created.Id, current.Id);
        Assert.Equal(created.PlayerId, current.PlayerId);
        Assert.Equal("Grommash", current.Name);
    }

    [Fact]
    public async Task GetCurrentAsync_FromANewServiceInstanceOverTheSameStore_ReturnsTheCreatedCharacter()
    {
        var (firstService, store) = CreateService();
        var created = await firstService.CreateNewGameAsync(new NewGameCharacterRequestDto
        {
            Name = "Grommash",
            Race = RaceType.Orc,
            ClassId = 2
        });

        var (secondService, _) = CreateService(store);
        var current = await secondService.GetCurrentAsync();

        Assert.Equal(created.Id, current.Id);
        Assert.Equal(created.PlayerId, current.PlayerId);
        Assert.Equal("Grommash", current.Name);
    }

    [Fact]
    public async Task GetCurrentAsync_WhenNoCharacterExists_ThrowsNotFound()
    {
        var (service, _) = CreateService();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetCurrentAsync());

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task GetCurrentAsync_WhenOnlyAnotherPlayersCharacterExists_ThrowsNotFound()
    {
        var (service, store) = CreateService();
        store.Characters.Add(new DnDGame.Domain.Entities.Characters.PlayerCharacter
        {
            Id = 7,
            OwnerId = "2",
            Name = "Borrowed",
            Level = 1,
            MaxHealth = 20,
            CurrentHealth = 20,
            RaceId = 1,
            ClassId = 1
        });

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetCurrentAsync());

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnerIdIsCorrupted_ThrowsInternalErrorInsteadOfMaskingWithPlayerZero()
    {
        var (service, store) = CreateService();
        var character = new DnDGame.Domain.Entities.Characters.PlayerCharacter
        {
            Id = 5,
            OwnerId = "not-an-int",
            Name = "Corrupted",
            Level = 1,
            MaxHealth = 20,
            CurrentHealth = 20,
            RaceId = 1,
            ClassId = 1
        };
        store.Characters.Add(character);

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetByIdAsync(character.Id));

        Assert.Equal(ErrorCodes.InternalError, exception.ErrorCode);
        Assert.Contains(character.Id.ToString(), exception.Message);
    }

    [Fact]
    public async Task GetOptionsAsync_ReturnsSeededRacesAndClasses()
    {
        var (service, _) = CreateService();

        var options = await service.GetOptionsAsync();

        Assert.Equal(4, options.Races.Count);
        Assert.Equal(4, options.Classes.Count);
        var orc = options.Races.Single(race => race.Name == "Orc");
        Assert.Equal(3, orc.Id);
        Assert.NotEmpty(orc.AttributeRanges);
        var warrior = options.Classes.Single(characterClass => characterClass.Name == "Warrior");
        Assert.NotEmpty(warrior.PrimaryAttribute);
    }

    private static (CharacterService Service, InMemoryGameDataStore Store) CreateService(InMemoryGameDataStore? store = null)
    {
        store ??= MockDataBootstrapper.CreateSeededStore();
        var service = new CharacterService(
            new MockCharacterRepository(store),
            new MockRaceRepository(store),
            new MockClassRepository(store),
            new MockCharacterPortraitRepository(store),
            new MockCurrentPlayerService());
        return (service, store);
    }
}
