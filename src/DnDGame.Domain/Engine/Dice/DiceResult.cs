namespace DnDGame.Domain.Engine.Dice;

/// <summary>
/// Immutable representation of a dice result. The final result is always derived
/// from the base roll and modifier.
/// </summary>
public sealed class DiceResult
{
    public DiceResult(DiceType diceType, int baseRoll, int modifier)
    {
        DiceType = diceType;
        BaseRoll = baseRoll;
        Modifier = modifier;
    }

    public DiceType DiceType { get; }

    public int BaseRoll { get; }

    public int Modifier { get; }

    public int FinalResult => BaseRoll + Modifier;
}
