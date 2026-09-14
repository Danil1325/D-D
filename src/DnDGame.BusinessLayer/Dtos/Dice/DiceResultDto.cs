using DnDGame.Domain.Engine.Dice;

namespace DnDGame.BusinessLayer.Dtos.Dice;

/// <summary>
/// Response shape for POST /api/dice/roll — mirrors DnDGame.Domain.Engine.Dice.DiceResult.
/// </summary>
public class DiceResultDto
{
    public DiceType DiceType { get; init; }

    public int BaseRoll { get; init; }

    public int Modifier { get; init; }

    public int FinalResult { get; init; }

    public static DiceResultDto FromDomain(DiceResult result) => new()
    {
        DiceType = result.DiceType,
        BaseRoll = result.BaseRoll,
        Modifier = result.Modifier,
        FinalResult = result.FinalResult
    };
}
