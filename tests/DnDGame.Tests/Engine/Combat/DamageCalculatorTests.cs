using DnDGame.Domain.Engine.Combat;
using Xunit;

namespace DnDGame.Tests.Engine.Combat;

public class DamageCalculatorTests
{
    private readonly IDamageCalculator _calculator = new DamageCalculator();

    [Fact]
    public void DamageCannotBeNegative()
    {
        var result = _calculator.Calculate(0, 100, 10);

        Assert.Equal(1, result.BaseDamage);
        Assert.Equal(1, result.BlockedDamage);
        Assert.Equal(0, result.FinalDamage);
        Assert.Equal(9, result.RemainingBlock);
    }

    [Fact]
    public void DamageMinimumIsOne()
    {
        var result = _calculator.Calculate(3, 10, 0);

        Assert.Equal(1, result.BaseDamage);
        Assert.Equal(0, result.BlockedDamage);
        Assert.Equal(1, result.FinalDamage);
        Assert.Equal(0, result.RemainingBlock);
    }

    [Fact]
    public void BlockReducesDamage()
    {
        var result = _calculator.Calculate(10, 0, 4);

        Assert.Equal(10, result.BaseDamage);
        Assert.Equal(4, result.BlockedDamage);
        Assert.Equal(6, result.FinalDamage);
        Assert.Equal(0, result.RemainingBlock);
    }

    [Fact]
    public void BlockGreaterThanDamagePreventsAllDamage()
    {
        var result = _calculator.Calculate(5, 0, 10);

        Assert.Equal(5, result.BaseDamage);
        Assert.Equal(5, result.BlockedDamage);
        Assert.Equal(0, result.FinalDamage);
        Assert.Equal(5, result.RemainingBlock);
    }
}
