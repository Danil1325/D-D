using DnDGame.BusinessLayer.Effects;
using Xunit;

namespace DnDGame.Tests.Effects;

public class AdditiveDamageCalculatorTests
{
    private readonly AdditiveDamageCalculator _calculator = new();

    [Fact]
    public void CalculateDamage_NoDefenseOrBlock_ReturnsBaseDamagePlusStrength()
    {
        var result = _calculator.CalculateDamage(baseDamage: 10, playerStrength: 5, enemyDefense: 0, enemyBlock: 0);

        Assert.Equal(15, result);
    }

    [Fact]
    public void CalculateDamage_DefenseReducesDamage_FlooredAtZero()
    {
        var result = _calculator.CalculateDamage(baseDamage: 10, playerStrength: 0, enemyDefense: 4, enemyBlock: 0);

        Assert.Equal(6, result);
    }

    [Fact]
    public void CalculateDamage_DefenseExceedsRawDamage_ReturnsZero()
    {
        var result = _calculator.CalculateDamage(baseDamage: 3, playerStrength: 0, enemyDefense: 10, enemyBlock: 0);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateDamage_BlockAbsorbsRemainingDamageAfterDefense()
    {
        var result = _calculator.CalculateDamage(baseDamage: 10, playerStrength: 0, enemyDefense: 2, enemyBlock: 5);

        Assert.Equal(3, result);
    }

    [Fact]
    public void CalculateDamage_BlockExceedsDamageAfterDefense_ReturnsZero()
    {
        var result = _calculator.CalculateDamage(baseDamage: 10, playerStrength: 0, enemyDefense: 2, enemyBlock: 100);

        Assert.Equal(0, result);
    }
}
