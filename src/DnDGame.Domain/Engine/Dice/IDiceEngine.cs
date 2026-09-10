using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.Dice;

/// <summary>
/// Generates dice results exclusively within the game backend.
/// </summary>
public interface IDiceEngine
{
    EngineResult<DiceResult> Roll(DiceType dice);

    EngineResult<DiceResult> RollWithModifier(DiceType dice, int modifier);
}
