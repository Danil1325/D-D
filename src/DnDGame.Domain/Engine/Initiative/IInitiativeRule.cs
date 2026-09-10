using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;

namespace DnDGame.Domain.Engine.Initiative;

/// <summary>
/// Calculates initiative scores from the combat rules configured for the game.
/// </summary>
public interface IInitiativeRule
{
    EngineResult<InitiativeScores> CalculateScores(
        BattleContext battleContext,
        IDiceEngine diceEngine);
}
