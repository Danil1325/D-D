using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;

namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Evaluates a pre-rolled dice result for a critical outcome.
/// </summary>
public interface ICriticalCalculator
{
    EngineResult<CriticalResult> Calculate(DiceResult diceResult);
}
