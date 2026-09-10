using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.Dice;

/// <summary>
/// Generates validated dice rolls through an injected random-number source.
/// </summary>
public sealed class DiceEngine : IDiceEngine
{
    private readonly IRandomNumberSource _randomNumberSource;

    public DiceEngine(IRandomNumberSource randomNumberSource)
    {
        ArgumentNullException.ThrowIfNull(randomNumberSource);
        _randomNumberSource = randomNumberSource;
    }

    public EngineResult<DiceResult> Roll(DiceType dice)
    {
        return RollWithModifier(dice, 0);
    }

    public EngineResult<DiceResult> RollWithModifier(DiceType dice, int modifier)
    {
        if (!Enum.IsDefined(dice))
        {
            return EngineResult<DiceResult>.Fail(
                "The requested dice type is invalid.",
                EngineErrorCodes.InvalidDice);
        }

        var sides = (int)dice;
        var baseRoll = _randomNumberSource.Next(1, sides + 1);
        if (baseRoll < 1 || baseRoll > sides)
        {
            return EngineResult<DiceResult>.Fail(
                "The random number source returned an invalid dice value.",
                EngineErrorCodes.InvalidDice);
        }

        return EngineResult<DiceResult>.Ok(
            new DiceResult(dice, baseRoll, modifier));
    }
}
