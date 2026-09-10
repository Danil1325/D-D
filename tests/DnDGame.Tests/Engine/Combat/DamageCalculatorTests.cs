using DnDGame.Domain.Engine.Combat;
using Xunit;

namespace DnDGame.Tests.Engine.Combat;

public class DamageCalculatorTests
{
    private readonly IDamageCalculator _calculator = new DamageCalculator(new AdditiveDamageRule());

    [Fact]
    public void DamageCalculationWorks()
    {
        var result = _calculator.Calculate(new DamageRequest(
            baseDamage: 10,
            strength: 2,
            defense: 3,
            block: 1,
            buffModifiers: new[] { 4 },
            debuffModifiers: new[] { 2 }));

        Assert.True(result.Success);
        Assert.Equal(12, result.Data!.RawDamage);
        Assert.Equal(14, result.Data.ModifiedDamage);
        Assert.Equal(11, result.Data.DamageAfterDefense);
        Assert.Equal(1, result.Data.BlockedDamage);
        Assert.Equal(10, result.Data.FinalDamage);
    }

    [Fact]
    public void BlockReducesDamage()
    {
        var result = _calculator.Calculate(new DamageRequest(10, 0, 0, 4));

        Assert.True(result.Success);
        Assert.Equal(4, result.Data!.BlockedDamage);
        Assert.Equal(6, result.Data.FinalDamage);
    }

    [Fact]
    public void DamageNeverNegative()
    {
        var result = _calculator.Calculate(new DamageRequest(5, 0, 100, 10));

        Assert.True(result.Success);
        Assert.Equal(0, result.Data!.FinalDamage);
    }

    [Fact]
    public void DefenseWorks()
    {
        var result = _calculator.Calculate(new DamageRequest(10, 0, 3, 0));

        Assert.True(result.Success);
        Assert.Equal(7, result.Data!.DamageAfterDefense);
        Assert.Equal(7, result.Data.FinalDamage);
    }

    [Fact]
    public void BuffModifierWorks()
    {
        var result = _calculator.Calculate(new DamageRequest(
            10,
            0,
            0,
            0,
            buffModifiers: new[] { 2, 3 }));

        Assert.True(result.Success);
        Assert.Equal(15, result.Data!.ModifiedDamage);
    }

    [Fact]
    public void DebuffModifierWorks()
    {
        var result = _calculator.Calculate(new DamageRequest(
            10,
            0,
            0,
            0,
            debuffModifiers: new[] { 3 }));

        Assert.True(result.Success);
        Assert.Equal(7, result.Data!.ModifiedDamage);
    }
}
