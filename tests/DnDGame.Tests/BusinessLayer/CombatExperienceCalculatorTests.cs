using DnDGame.API.CompositionRoot;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Enemies;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DnDGame.Tests.BusinessLayer;

public class CombatExperienceCalculatorTests
{
    private static CombatExperienceCalculator CreateCalculator() =>
        new(new ExperienceService(new LevelProgressionRules()));

    private static (Battle Battle, Enemy Enemy, GameSession Session, PlayerCharacter Player) CreateScenario()
    {
        var player = new PlayerCharacter { Id = 1 };
        var session = new GameSession { Id = 2, CharacterId = player.Id };
        var enemy = new Enemy { Id = 3, Family = EnemyFamily.Goblin, Tier = EnemyTier.Base };
        var battle = new Battle
        {
            Id = 4, GameSessionId = session.Id, EnemyId = enemy.Id, Status = GameSessionStatus.Victory
        };
        return (battle, enemy, session, player);
    }

    [Theory]
    [InlineData(EnemyFamily.Goblin, EnemyTier.Base, 20)]
    [InlineData(EnemyFamily.Skeleton, EnemyTier.Base, 20)]
    [InlineData(EnemyFamily.Slime, EnemyTier.Base, 20)]
    [InlineData(EnemyFamily.Wraith, EnemyTier.Base, 20)]
    [InlineData(EnemyFamily.Goblin, EnemyTier.Evolved, 45)]
    [InlineData(EnemyFamily.Skeleton, EnemyTier.Evolved, 45)]
    [InlineData(EnemyFamily.Slime, EnemyTier.Evolved, 45)]
    [InlineData(EnemyFamily.Wraith, EnemyTier.Evolved, 45)]
    [InlineData(EnemyFamily.Demon, EnemyTier.Base, 55)]
    [InlineData(EnemyFamily.Troll, EnemyTier.Base, 55)]
    [InlineData(EnemyFamily.Chimera, EnemyTier.Base, 55)]
    [InlineData(EnemyFamily.Demon, EnemyTier.Evolved, 90)]
    [InlineData(EnemyFamily.Troll, EnemyTier.Evolved, 90)]
    [InlineData(EnemyFamily.Chimera, EnemyTier.Evolved, 90)]
    [InlineData(EnemyFamily.Goblin, EnemyTier.Elite, 160)]
    [InlineData(EnemyFamily.Skeleton, EnemyTier.Elite, 160)]
    [InlineData(EnemyFamily.Slime, EnemyTier.Elite, 160)]
    [InlineData(EnemyFamily.Wraith, EnemyTier.Elite, 160)]
    [InlineData(EnemyFamily.Troll, EnemyTier.Elite, 160)]
    [InlineData(EnemyFamily.Chimera, EnemyTier.Elite, 160)]
    public void CalculateAndAward_AllSpecifiedRewards(EnemyFamily family, EnemyTier tier, int expected)
    {
        var (battle, enemy, session, player) = CreateScenario();
        enemy.Family = family;
        enemy.Tier = tier;
        enemy.Name = "Display name does not determine rewards";
        enemy.XpReward = 1;

        var result = CreateCalculator().CalculateAndAward(battle, enemy, session, player);

        Assert.Equal(expected, result.BaseExperience);
        Assert.Equal(expected, result.ExperienceGained);
        Assert.Equal(expected, player.CurrentXp);
        Assert.True(result.WasGranted);
        Assert.True(battle.RewardsGranted);
        Assert.Contains(battle.Id, session.ExperienceRewardedBattleIds);
    }

    [Fact]
    public void CalculateAndAward_RetryWithNewCalculatorAndBattleCopy_DoesNotPayTwice()
    {
        var (battle, enemy, session, player) = CreateScenario();
        CreateCalculator().CalculateAndAward(battle, enemy, session, player);
        var copy = new Battle
        {
            Id = battle.Id, GameSessionId = session.Id, EnemyId = enemy.Id, Status = GameSessionStatus.Victory
        };

        var result = CreateCalculator().CalculateAndAward(copy, enemy, session, player);

        Assert.True(result.AlreadyGranted);
        Assert.False(result.WasGranted);
        Assert.Equal(0, result.ExperienceGained);
        Assert.Equal(20, player.CurrentXp);
    }

    [Fact]
    public void CalculateAndAward_ExistingRewardMarker_DoesNotPayAgain()
    {
        var (battle, enemy, session, player) = CreateScenario();
        battle.RewardsGranted = true;
        var result = CreateCalculator().CalculateAndAward(battle, enemy, session, player);
        Assert.True(result.AlreadyGranted);
        Assert.Equal(0, player.CurrentXp);
    }

    [Theory]
    [InlineData(GameSessionStatus.InProgress)]
    [InlineData(GameSessionStatus.Abandoned)]
    [InlineData(GameSessionStatus.Defeat)]
    public void CalculateAndAward_UnsuccessfulBattle_DoesNotPayOrConsumeClaim(GameSessionStatus status)
    {
        var (battle, enemy, session, player) = CreateScenario();
        battle.Status = status;

        var result = CreateCalculator().CalculateAndAward(battle, enemy, session, player);

        Assert.False(result.WasGranted);
        Assert.Equal(0, player.CurrentXp);
        Assert.False(battle.RewardsGranted);
        Assert.Empty(session.ExperienceRewardedBattleIds);
    }

    [Fact]
    public void CalculateAndAward_PermanentNonCombatResolution_PaysAndEndsEncounterOnlyOnce()
    {
        var (battle, enemy, session, player) = CreateScenario();
        battle.Status = GameSessionStatus.InProgress;
        var calculator = CreateCalculator();

        var result = calculator.CalculateAndAward(battle, enemy, session, player, true);
        var retry = calculator.CalculateAndAward(battle, enemy, session, player);

        Assert.Equal(20, result.ExperienceGained);
        Assert.Equal(GameSessionStatus.Victory, battle.Status);
        Assert.NotNull(battle.CompletedAt);
        Assert.True(retry.AlreadyGranted);
        Assert.Equal(20, player.CurrentXp);
    }

    [Theory]
    [InlineData(GameSessionStatus.Abandoned)]
    [InlineData(GameSessionStatus.Defeat)]
    public void CalculateAndAward_NonCombatFlagCannotOverrideFailure(GameSessionStatus status)
    {
        var (battle, enemy, session, player) = CreateScenario();
        battle.Status = status;
        Assert.False(CreateCalculator().CalculateAndAward(battle, enemy, session, player, true).WasGranted);
        Assert.Equal(status, battle.Status);
        Assert.Equal(0, player.CurrentXp);
    }

    [Fact]
    public void CalculateAndAward_AbandonedSession_DoesNotPay()
    {
        var (battle, enemy, session, player) = CreateScenario();
        session.Status = GameSessionStatus.Abandoned;
        Assert.False(CreateCalculator().CalculateAndAward(battle, enemy, session, player).WasGranted);
        Assert.Equal(0, player.CurrentXp);
    }

    [Fact]
    public void CalculateAndAward_RepeatedSummonerAcrossBattles_PaysOnlyFirstSummon()
    {
        var (battle, enemy, session, player) = CreateScenario();
        battle.SummonerInstanceId = Guid.NewGuid();
        CreateCalculator().CalculateAndAward(battle, enemy, session, player);
        var second = new Battle
        {
            Id = 5, GameSessionId = session.Id, EnemyId = enemy.Id, Status = GameSessionStatus.Victory,
            SummonerInstanceId = battle.SummonerInstanceId
        };
        var result = CreateCalculator().CalculateAndAward(second, enemy, session, player);

        Assert.True(result.RepeatedSummon);
        Assert.False(result.WasGranted);
        Assert.Equal(0, result.ExperienceGained);
        Assert.Equal(20, player.CurrentXp);
        Assert.True(second.RewardsGranted);
        Assert.Equal(2, session.ExperienceRewardedBattleIds.Count);
        Assert.Single(session.ExperienceRewardedSummonerIds);
    }

    [Fact]
    public void CalculateAndAward_DistinctSummonersAndOrdinaryEnemies_AllPay()
    {
        var (battle, enemy, session, player) = CreateScenario();
        for (var index = 0; index < 4; index++)
        {
            battle = new Battle
            {
                Id = 10 + index, GameSessionId = session.Id, EnemyId = enemy.Id,
                Status = GameSessionStatus.Victory,
                SummonerInstanceId = index < 2 ? Guid.NewGuid() : null
            };
            Assert.True(CreateCalculator().CalculateAndAward(battle, enemy, session, player).WasGranted);
        }
        Assert.Equal(80, player.CurrentXp);
    }

    [Fact]
    public void CalculateAndAward_FailedSummonDoesNotConsumeSummonerReward()
    {
        var (battle, enemy, session, player) = CreateScenario();
        battle.SummonerInstanceId = Guid.NewGuid();
        battle.Status = GameSessionStatus.Abandoned;
        CreateCalculator().CalculateAndAward(battle, enemy, session, player);
        Assert.Empty(session.ExperienceRewardedSummonerIds);

        var next = new Battle
        {
            Id = 5, GameSessionId = session.Id, EnemyId = enemy.Id, Status = GameSessionStatus.Victory,
            SummonerInstanceId = battle.SummonerInstanceId
        };
        Assert.Equal(20, CreateCalculator().CalculateAndAward(next, enemy, session, player).ExperienceGained);
    }

    [Fact]
    public void CalculateAndAward_ProgressionFailure_DoesNotConsumeReward()
    {
        var (battle, enemy, session, player) = CreateScenario();
        player.Level = 0;
        Assert.Throws<ArgumentException>(() =>
            CreateCalculator().CalculateAndAward(battle, enemy, session, player));
        Assert.False(battle.RewardsGranted);
        Assert.Empty(session.ExperienceRewardedBattleIds);
    }

    [Fact]
    public void CalculateAndAward_LevelUp_UsesExperienceService()
    {
        var (battle, enemy, session, player) = CreateScenario();
        player.CurrentXp = 110;
        var result = CreateCalculator().CalculateAndAward(battle, enemy, session, player);
        Assert.Equal(130, player.CurrentXp);
        Assert.Equal(2, player.Level);
        Assert.Equal(3, player.SkillPoints);
        Assert.True(result.Progression!.DidLevelUp);
    }

    [Fact]
    public async Task CalculateAndAward_ConcurrentClaims_PayOnce()
    {
        var (battle, enemy, session, player) = CreateScenario();
        var results = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
            CreateCalculator().CalculateAndAward(battle, enemy, session, player))));
        Assert.Single(results, result => result.WasGranted);
        Assert.Equal(20, player.CurrentXp);
    }

    [Fact]
    public void CalculateAndAward_MismatchedCharacter_IsRejected()
    {
        var (battle, enemy, session, player) = CreateScenario();
        player.Id = 999;
        Assert.Throws<ArgumentException>(() =>
            CreateCalculator().CalculateAndAward(battle, enemy, session, player));
        Assert.Equal(0, player.CurrentXp);
    }

    [Fact]
    public void GetBaseExperience_UnlistedDemonLord_PreservesConfiguredReward()
    {
        var enemy = new Enemy { Family = EnemyFamily.Demon, Tier = EnemyTier.Elite, XpReward = 75 };
        Assert.Equal(75, CreateCalculator().GetBaseExperience(enemy));
    }

    [Fact]
    public void DependencyInjection_ResolvesCalculator()
    {
        var services = new ServiceCollection();
        services.AddCardBattleServices();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ICombatExperienceCalculator>());
    }
}
