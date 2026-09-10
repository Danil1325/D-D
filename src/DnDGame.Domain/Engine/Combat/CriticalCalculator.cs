using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;

namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Deterministically evaluates a pre-rolled D20 against configurable critical rules.
/// </summary>
public sealed class CriticalCalculator : ICriticalCalculator
{
    private readonly CriticalRules _rules;

    public CriticalCalculator(CriticalRules? rules = null)
    {
        _rules = rules ?? new CriticalRules();
    }

    public EngineResult<CriticalResult> Calculate(DiceResult diceResult)
    {
        ArgumentNullException.ThrowIfNull(diceResult);

        var outcome = DetermineOutcome(diceResult);
        return EngineResult<CriticalResult>.Ok(new CriticalResult(outcome, diceResult));
    }

    private CriticalOutcome DetermineOutcome(DiceResult diceResult)
    {
        if (diceResult.DiceType != DiceType.D20)
        {
            return CriticalOutcome.NormalHit;
        }

        if (diceResult.BaseRoll >= _rules.CriticalHitThreshold)
        {
            return CriticalOutcome.CriticalHit;
        }

        if (diceResult.BaseRoll <= _rules.CriticalMissThreshold)
        {
            return CriticalOutcome.CriticalMiss;
        }

        return CriticalOutcome.NormalHit;
    }
}
