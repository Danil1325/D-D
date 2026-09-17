using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;

namespace DnDGame.Domain.Engine.Initiative;

/// <summary>
/// Configurable MVP rule, same additive style as <see cref="Combat.AdditiveDamageRule"/>:
/// each side rolls a d20 and adds a flat modifier — the player's Dexterity, the enemy's
/// Defense (enemies have no Dexterity attribute; Defense is the closest existing stat
/// representing how hard they are to catch off guard). Higher score acts first;
/// InitiativeEngine handles the tie case itself.
/// </summary>
public sealed class AdditiveInitiativeRule : IInitiativeRule
{
    public EngineResult<InitiativeScores> CalculateScores(BattleContext battleContext, IDiceEngine diceEngine)
    {
        ArgumentNullException.ThrowIfNull(battleContext);
        ArgumentNullException.ThrowIfNull(diceEngine);

        var playerRoll = diceEngine.Roll(DiceType.D20);
        if (!playerRoll.Success || playerRoll.Data is null)
        {
            return EngineResult<InitiativeScores>.Fail(
                playerRoll.Message,
                playerRoll.ErrorCode ?? EngineErrorCodes.MissingCombatRule);
        }

        var enemyRoll = diceEngine.Roll(DiceType.D20);
        if (!enemyRoll.Success || enemyRoll.Data is null)
        {
            return EngineResult<InitiativeScores>.Fail(
                enemyRoll.Message,
                enemyRoll.ErrorCode ?? EngineErrorCodes.MissingCombatRule);
        }

        var playerDice = playerRoll.Data.BaseRoll;
        var enemyDice = enemyRoll.Data.BaseRoll;

        return EngineResult<InitiativeScores>.Ok(
            new InitiativeScores(
                battleContext.Player.Dexterity + playerDice,
                battleContext.Enemy.Defense + enemyDice,
                playerDice,
                enemyDice));
    }
}
