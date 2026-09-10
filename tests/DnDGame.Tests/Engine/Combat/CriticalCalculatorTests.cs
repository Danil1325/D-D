using DnDGame.Domain.Engine.Combat;
using DnDGame.Domain.Engine.Dice;
using Xunit;

namespace DnDGame.Tests.Engine.Combat;

public class CriticalCalculatorTests
{
    [Fact]
    public void CriticalHitWorks()
    {
        var calculator = new CriticalCalculator();

        var result = calculator.Calculate(new DiceResult(DiceType.D20, 20, 0));

        Assert.True(result.Success);
        Assert.Equal(CriticalOutcome.CriticalHit, result.Data!.Outcome);
    }

    [Fact]
    public void CriticalMissWorks()
    {
        var calculator = new CriticalCalculator();

        var result = calculator.Calculate(new DiceResult(DiceType.D20, 1, 0));

        Assert.True(result.Success);
        Assert.Equal(CriticalOutcome.CriticalMiss, result.Data!.Outcome);
    }

    [Fact]
    public void NormalHitWorks()
    {
        var calculator = new CriticalCalculator();

        var result = calculator.Calculate(new DiceResult(DiceType.D20, 14, 0));

        Assert.True(result.Success);
        Assert.Equal(CriticalOutcome.NormalHit, result.Data!.Outcome);
    }

    [Fact]
    public void ConfiguredRulesChangeCriticalOutcome()
    {
        var calculator = new CriticalCalculator(new CriticalRules(
            CriticalHitThreshold: 18,
            CriticalMissThreshold: 2));

        var criticalHit = calculator.Calculate(new DiceResult(DiceType.D20, 18, 0));
        var criticalMiss = calculator.Calculate(new DiceResult(DiceType.D20, 2, 0));

        Assert.Equal(CriticalOutcome.CriticalHit, criticalHit.Data!.Outcome);
        Assert.Equal(CriticalOutcome.CriticalMiss, criticalMiss.Data!.Outcome);
    }
}
