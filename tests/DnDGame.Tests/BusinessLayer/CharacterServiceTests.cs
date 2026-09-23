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

    private static (CharacterService Service, InMemoryGameDataStore Store) CreateService()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var service = new CharacterService(
            new MockCharacterRepository(store),
            new MockRaceRepository(store),
            new MockClassRepository(store),
            new MockCharacterPortraitRepository(store),
            new MockCurrentPlayerService());
        return (service, store);
    }
}
