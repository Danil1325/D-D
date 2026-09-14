using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.EnemyActions;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Enemies;

namespace DnDGame.Tests.Engine.EnemyActions;

public class EnemyDefenseRuleTests
{
    private readonly EnemyDefenseRule _rule = new();

    [Theory]
    [InlineData(8, 4)]
    [InlineData(9, 4)]
    [InlineData(7, 3)]
    [InlineData(6, 3)]
    [InlineData(18, 9)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    public void CalculateBlockReturnsHalfTheDefenseStatRoundedDown(int defense, int expectedBlock)
    {
        var context = new BattleContext(
            new PlayerCharacter(),
            new Enemy { Defense = defense },
            new BattleState());

        var result = _rule.CalculateBlock(context);

        Assert.True(result.Success);
        Assert.Equal(expectedBlock, result.Data);
    }

    [Fact]
    public void CalculateBlockAlwaysSucceeds()
    {
        var context = new BattleContext(
            new PlayerCharacter(),
            new Enemy { Defense = 11 },
            new BattleState());

        var result = _rule.CalculateBlock(context);

        Assert.True(result.Success);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public void CalculateBlockThrowsOnNullContext()
    {
        Assert.Throws<ArgumentNullException>(() => _rule.CalculateBlock(null!));
    }
}