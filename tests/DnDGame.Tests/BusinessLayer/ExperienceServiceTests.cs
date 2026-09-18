using DnDGame.API.CompositionRoot;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using Microsoft.Extensions.DependencyInjection;

namespace DnDGame.Tests.BusinessLayer;

public class ExperienceServiceTests
{
    private readonly ExperienceService _service = new(new LevelProgressionRules());

    [Fact]
    public void AddExperience_OneLevel_PreservesExistingPointsAndCumulativeExperience()
    {
        var player = new PlayerCharacter { CurrentXp = 100, SkillPoints = 4 };

        var result = _service.AddExperience(player, 30);

        Assert.Equal(1, result.PreviousLevel);
        Assert.Equal(2, result.CurrentLevel);
        Assert.Equal(30, result.ExperienceGained);
        Assert.Equal(130, result.TotalExperience);
        Assert.Equal(1, result.LevelsGained);
        Assert.Equal(3, result.SkillPointsGained);
        Assert.True(result.DidLevelUp);
        Assert.Equal(2, player.Level);
        Assert.Equal(130, player.CurrentXp);
        Assert.Equal(7, player.SkillPoints);
    }

    [Fact]
    public void AddExperience_MultipleLevels_AwardsPointsForEveryLevel()
    {
        var player = new PlayerCharacter { Level = 2, CurrentXp = 120, SkillPoints = 3 };

        var result = _service.AddExperience(player, 730);

        Assert.Equal(2, result.PreviousLevel);
        Assert.Equal(5, result.CurrentLevel);
        Assert.Equal(3, result.LevelsGained);
        Assert.Equal(9, result.SkillPointsGained);
        Assert.Equal(730, result.ExperienceGained);
        Assert.Equal(850, result.TotalExperience);
        Assert.True(result.DidLevelUp);
        Assert.Equal(5, player.Level);
        Assert.Equal(850, player.CurrentXp);
        Assert.Equal(12, player.SkillPoints);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(119, 1)]
    [InlineData(120, 2)]
    [InlineData(299, 2)]
    [InlineData(300, 3)]
    [InlineData(549, 3)]
    [InlineData(550, 4)]
    [InlineData(849, 4)]
    [InlineData(850, 5)]
    [InlineData(1199, 5)]
    [InlineData(1200, 6)]
    [InlineData(1649, 6)]
    [InlineData(1650, 7)]
    [InlineData(2149, 7)]
    [InlineData(2150, 8)]
    [InlineData(2749, 8)]
    [InlineData(2750, 9)]
    [InlineData(3449, 9)]
    [InlineData(3450, 10)]
    [InlineData(10000, 10)]
    public void AddExperience_ThresholdBoundaries(int experience, int expectedLevel)
    {
        var player = new PlayerCharacter();
        var result = _service.AddExperience(player, experience);

        Assert.Equal(expectedLevel, player.Level);
        Assert.Equal(experience, player.CurrentXp);
        Assert.Equal((expectedLevel - 1) * 3, player.SkillPoints);
        Assert.Equal(expectedLevel > 1, result.DidLevelUp);
    }

    [Fact]
    public void AddExperience_AtMaximumLevel_RetainsExtraExperienceWithoutMorePoints()
    {
        var player = new PlayerCharacter { Level = 10, CurrentXp = 3450, SkillPoints = 27 };

        var result = _service.AddExperience(player, 500);

        Assert.Equal(3950, result.TotalExperience);
        Assert.Equal(500, result.ExperienceGained);
        Assert.Equal(10, player.Level);
        Assert.Equal(27, player.SkillPoints);
        Assert.Equal(0, result.LevelsGained);
        Assert.Equal(0, result.SkillPointsGained);
        Assert.False(result.DidLevelUp);
    }

    [Fact]
    public void AddExperience_HugeRepeatedRewards_SaturateWithoutOverflow()
    {
        var player = new PlayerCharacter { CurrentXp = 100 };
        var first = _service.AddExperience(player, int.MaxValue);
        var second = _service.AddExperience(player, int.MaxValue);

        Assert.Equal(int.MaxValue - 100, first.ExperienceGained);
        Assert.Equal(0, second.ExperienceGained);
        Assert.Equal(int.MaxValue, second.TotalExperience);
        Assert.Equal(10, player.Level);
        Assert.Equal(27, player.SkillPoints);
        Assert.False(second.DidLevelUp);
    }

    [Fact]
    public void AddExperience_QuestThenBattle_AccumulatesWithoutDuplicatingLevelPoints()
    {
        var player = new PlayerCharacter();
        var reward = new QuestReward { Experience = 120 };
        _service.AddExperience(player, reward.Experience);
        var result = _service.AddExperience(player, 180);
        var noReward = _service.AddExperience(player, 0);

        Assert.Equal(3, player.Level);
        Assert.Equal(300, player.CurrentXp);
        Assert.Equal(6, player.SkillPoints);
        Assert.Equal(3, result.SkillPointsGained);
        Assert.Equal(0, noReward.SkillPointsGained);
        Assert.False(noReward.DidLevelUp);
    }

    [Fact]
    public void AddExperience_CustomRules_UsesConfiguredThresholdsAndPoints()
    {
        var service = new ExperienceService(new LevelProgressionRules(
            new[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90 }, 5));
        var player = new PlayerCharacter();

        var result = service.AddExperience(player, 20);

        Assert.Equal(3, result.CurrentLevel);
        Assert.Equal(10, player.SkillPoints);
    }

    [Fact]
    public void AddExperience_NegativeReward_RejectsWithoutMutation()
    {
        var player = new PlayerCharacter { CurrentXp = 100, SkillPoints = 4 };

        Assert.Throws<ArgumentOutOfRangeException>(() => _service.AddExperience(player, -1));
        Assert.Equal(1, player.Level);
        Assert.Equal(100, player.CurrentXp);
        Assert.Equal(4, player.SkillPoints);
    }

    [Fact]
    public void AddExperience_NullPlayer_IsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => _service.AddExperience(null!, 10));
    }

    [Fact]
    public void Rules_InvalidThresholds_AreRejected()
    {
        Assert.Throws<ArgumentException>(() => new LevelProgressionRules(new[] { 0, 120 }));
        Assert.Throws<ArgumentException>(() => new LevelProgressionRules(
            new[] { 1, 120, 300, 550, 850, 1200, 1650, 2150, 2750, 3450 }));
        Assert.Throws<ArgumentException>(() => new LevelProgressionRules(
            new[] { 0, 120, 120, 550, 850, 1200, 1650, 2150, 2750, 3450 }));
        Assert.Throws<ArgumentException>(() => new LevelProgressionRules(
            new[] { 0, 120, 100, 550, 850, 1200, 1650, 2150, 2750, 3450 }));
    }

    [Fact]
    public void DependencyInjection_ResolvesExperienceService()
    {
        var services = new ServiceCollection();
        services.AddCardBattleServices();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<IExperienceService>();
        Assert.Equal(2, service.AddExperience(new PlayerCharacter(), 120).CurrentLevel);
    }
}
