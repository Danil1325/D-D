using DnDGame.Domain.Engine.Dice;

namespace DnDGame.Domain.Engine.SavingThrows;

/// <summary>
/// Result of a saving throw, including a one-time marker for downstream effect
/// integration.
/// </summary>
public sealed class SavingThrowResult
{
    internal SavingThrowResult(DiceResult diceResult, int difficultyClass, int modifier)
    {
        DiceResult = diceResult;
        DifficultyClass = difficultyClass;
        Modifier = modifier;
        FinalResult = diceResult.BaseRoll + modifier;
        Success = FinalResult >= difficultyClass;
    }

    public DiceResult DiceResult { get; }

    public int BaseRoll => DiceResult.BaseRoll;

    public int DifficultyClass { get; }

    public int Modifier { get; }

    public int FinalResult { get; }

    public bool Success { get; }

    public bool Failure => !Success;

    public bool ConsequenceApplied { get; private set; }

    internal void MarkConsequenceApplied()
    {
        ConsequenceApplied = true;
    }
}
