using DnDGame.Domain.Engine.Dice;

namespace DnDGame.BusinessLayer.Dtos.Dice;

/// <summary>
/// Request shape for POST /api/dice/roll.
/// </summary>
public class DiceRequestDto
{
    public DiceType DiceType { get; set; }

    /// <summary>Added to the base roll to produce FinalResult. Defaults to 0 (a plain roll).</summary>
    public int Modifier { get; set; }
}
