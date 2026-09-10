using DnDGame.Domain.Engine.Combat;
using DnDGame.Domain.Engine.Common;
using Xunit;

namespace DnDGame.Tests.Engine.Combat;

public class DodgeCalculatorTests
{
    [Fact]
    public void DodgeWorks()
    {
        var calculator = new DodgeCalculator(new FixedDodgeRule(DodgeOutcome.Dodge));

        var result = calculator.Calculate(new DodgeRequest(5, 10));

        Assert.True(result.Success);
        Assert.Equal(DodgeOutcome.Dodge, result.Data!.Outcome);
        Assert.True(result.Data.IsDodge);
    }

    [Fact]
    public void HitWorks()
    {
        var calculator = new DodgeCalculator(new FixedDodgeRule(DodgeOutcome.Hit));

        var result = calculator.Calculate(new DodgeRequest(5, 10));

        Assert.True(result.Success);
        Assert.Equal(DodgeOutcome.Hit, result.Data!.Outcome);
        Assert.False(result.Data.IsDodge);
    }

    [Fact]
    public void MissingRuleReturnsControlledError()
    {
        var calculator = new DodgeCalculator();

        var result = calculator.Calculate(new DodgeRequest(5, 10));

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.MissingCombatRule, result.ErrorCode);
    }

    private sealed class FixedDodgeRule : IDodgeRule
    {
        private readonly DodgeOutcome _outcome;

        public FixedDodgeRule(DodgeOutcome outcome)
        {
            _outcome = outcome;
        }

        public EngineResult<DodgeOutcome> Resolve(DodgeRequest request)
        {
            return EngineResult<DodgeOutcome>.Ok(_outcome);
        }
    }
}
