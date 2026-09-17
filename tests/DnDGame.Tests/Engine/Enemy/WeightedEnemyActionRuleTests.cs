using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Dice;
using DnDGame.Domain.Engine.EnemyActions;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Enemies;
using Xunit;

namespace DnDGame.Tests.Engine.EnemyActions;

public class WeightedEnemyActionRuleTests
{
    [Theory]
    [InlineData(0, EnemyActionType.Attack)]
    [InlineData(WeightedEnemyActionRule.AttackChancePercent - 1, EnemyActionType.Attack)]
    [InlineData(WeightedEnemyActionRule.AttackChancePercent, EnemyActionType.Defend)]
    [InlineData(99, EnemyActionType.Defend)]
    public void SelectAction_UsesRandomNumberSourceToWeightTowardAttack(int roll, EnemyActionType expected)
    {
        var rule = new WeightedEnemyActionRule(new FixedRandomNumberSource(roll));

        var result = rule.SelectAction(CreateBattleContext());

        Assert.True(result.Success);
        Assert.Equal(expected, result.Data!.Type);
    }

    private static BattleContext CreateBattleContext() =>
        new(new PlayerCharacter(), new DnDGame.Domain.Entities.Enemies.Enemy(), new BattleState());

    private sealed class FixedRandomNumberSource : IRandomNumberSource
    {
        private readonly int _value;

        public FixedRandomNumberSource(int value)
        {
            _value = value;
        }

        public int Next(int minimumInclusive, int maximumExclusive) => _value;
    }
}
