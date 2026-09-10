using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;
using DnDGame.Domain.Engine.Enums;

namespace DnDGame.Domain.Engine.Initiative;

/// <summary>
/// Determines the first turn from scores supplied by a configurable initiative rule.
/// </summary>
public sealed class InitiativeEngine : IInitiativeEngine
{
    private readonly IDiceEngine _diceEngine;
    private readonly IInitiativeRule? _initiativeRule;

    public InitiativeEngine(IDiceEngine diceEngine, IInitiativeRule? initiativeRule = null)
    {
        ArgumentNullException.ThrowIfNull(diceEngine);

        _diceEngine = diceEngine;
        _initiativeRule = initiativeRule;
    }

    public EngineResult<InitiativeResult> DetermineFirstTurn(BattleContext battleContext)
    {
        if (_initiativeRule is null)
        {
            return MissingRule("No initiative rule has been configured.");
        }

        var scoresResult = _initiativeRule.CalculateScores(battleContext, _diceEngine);
        if (!scoresResult.Success || scoresResult.Data is null)
        {
            return EngineResult<InitiativeResult>.Fail(
                scoresResult.Message,
                scoresResult.ErrorCode ?? EngineErrorCodes.MissingCombatRule);
        }

        var scores = scoresResult.Data;
        if (scores.PlayerScore == scores.EnemyScore)
        {
            return MissingRule("No initiative tie-break rule has been configured.");
        }

        var startingTurn = scores.PlayerScore > scores.EnemyScore
            ? TurnType.Player
            : TurnType.Enemy;

        return EngineResult<InitiativeResult>.Ok(
            new InitiativeResult(
                startingTurn,
                scores.PlayerScore,
                scores.EnemyScore,
                scores.PlayerDiceResult,
                scores.EnemyDiceResult));
    }

    private static EngineResult<InitiativeResult> MissingRule(string message)
    {
        return EngineResult<InitiativeResult>.Fail(
            message,
            EngineErrorCodes.MissingCombatRule);
    }
}
