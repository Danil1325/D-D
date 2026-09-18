using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Characters;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using Xunit;

namespace DnDGame.Tests.BusinessLayer;

public class ProgressionServiceTests
{
    private const int PlayerId = 1;

    // --- Baseline readout ---

    [Fact]
    public async Task GetProgression_AtLevelOneWithNoExperience_ReportsBaseline()
    {
        var scenario = CreateScenario(level: 1, xp: 0);

        var result = await scenario.Service.GetProgressionAsync(PlayerId);

        Assert.Equal(1, result.Level);
        Assert.Equal(0, result.CurrentExperience);
        Assert.Equal(0, result.ExperienceForCurrentLevel);
        Assert.Equal(120, result.ExperienceForNextLevel);
        Assert.Equal(0, result.ExperienceProgressPercentage);
        Assert.Equal(0, result.AvailableSkillPoints);
    }

    [Fact]
    public async Task GetProgression_WhenHalfwayThroughLevel_ReportsPercentage()
    {
        var scenario = CreateScenario(level: 1, xp: 60);

        var result = await scenario.Service.GetProgressionAsync(PlayerId);

        Assert.Equal(50, result.ExperienceProgressPercentage);
    }

    [Fact]
    public async Task GetProgression_AtMaxLevel_ReportsNoNextTier()
    {
        var scenario = CreateScenario(level: 10, xp: 4000);

        var result = await scenario.Service.GetProgressionAsync(PlayerId);

        Assert.Equal(10, result.Level);
        Assert.Equal(3450, result.ExperienceForCurrentLevel);
        Assert.Null(result.ExperienceForNextLevel);
        Assert.Equal(0, result.ExperienceProgressPercentage);
    }

    [Fact]
    public async Task GetProgression_UnknownPlayer_FailsWithNotFound()
    {
        var scenario = CreateScenario();

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => scenario.Service.GetProgressionAsync(99));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    // --- Granting experience ---

    [Fact]
    public async Task GrantExperience_LevelsUpAndPersistsTheCharacter()
    {
        var scenario = CreateScenario(level: 1, xp: 0);

        var result = await scenario.Service.GrantExperienceAsync(PlayerId, 120);

        Assert.Equal(2, result.Level);
        Assert.Equal(120, result.CurrentExperience);
        Assert.Equal(120, result.ExperienceForCurrentLevel);
        Assert.Equal(300, result.ExperienceForNextLevel);
        Assert.Equal(3, result.AvailableSkillPoints);
        Assert.Equal(120, scenario.Character.CurrentXp);
        Assert.Equal(3, scenario.Character.SkillPoints);
    }

    [Fact]
    public async Task GrantExperience_AccumulatesWithoutLeveling_PersistsProgress()
    {
        var scenario = CreateScenario(level: 1, xp: 0);

        var result = await scenario.Service.GrantExperienceAsync(PlayerId, 40);

        Assert.Equal(1, result.Level);
        Assert.Equal(40, result.CurrentExperience);
        Assert.Equal(33.3, result.ExperienceProgressPercentage, 2);
        Assert.Equal(40, scenario.Character.CurrentXp);
    }

    [Fact]
    public async Task GrantExperience_WithZeroOrNegativeAmount_FailsWithValidationError()
    {
        var scenario = CreateScenario();

        var zero = await Assert.ThrowsAsync<DomainException>(
            () => scenario.Service.GrantExperienceAsync(PlayerId, 0));
        var negative = await Assert.ThrowsAsync<DomainException>(
            () => scenario.Service.GrantExperienceAsync(PlayerId, -5));

        Assert.Equal(ErrorCodes.ValidationError, zero.ErrorCode);
        Assert.Equal(ErrorCodes.ValidationError, negative.ErrorCode);
    }

    [Fact]
    public async Task GrantExperience_UnknownPlayer_FailsWithNotFound()
    {
        var scenario = CreateScenario();

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => scenario.Service.GrantExperienceAsync(99, 100));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    // --- Scenario helpers ---

    private static (IProgressionService Service, InMemoryGameDataStore Store, PlayerCharacter Character) CreateScenario(
        int level = 1,
        int xp = 0)
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var character = new PlayerCharacter
        {
            Id = PlayerId,
            OwnerId = PlayerId.ToString(),
            Name = "Hero",
            Level = 1,
            CurrentXp = 0,
            SkillPoints = 0,
            MaxHealth = 20,
            CurrentHealth = 20,
            RaceId = 1,
            ClassId = 1
        };
        character.Level = level;
        character.CurrentXp = xp;
        store.Characters.Add(character);

        var service = new ProgressionService(
            new MockCharacterRepository(store),
            new ExperienceService(new LevelProgressionRules()),
            new LevelProgressionRules());

        return (service, store, character);
    }
}