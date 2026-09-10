using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Initiative;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Enemies;
using Xunit;

namespace DnDGame.Tests.Engine.Initiative;

public class InitiativeEngineTests
{
    [Fact]
    public void InitiativeDeterminesFirstTurn()
    {
        var engine = new InitiativeEngine(new FixedDiceEngine(4), new DeterministicInitiativeRule());

        var result = engine.DetermineFirstTurn(CreateBattleContext());

        Assert.True(result.Success);
        Assert.Equal(TurnType.Enemy, result.Data!.StartingTurn);
        Assert.Equal(9, result.Data.PlayerScore);
        Assert.Equal(14, result.Data.EnemyScore);
        Assert.Equal(4, result.Data.PlayerDiceResult);
        Assert.Equal(4, result.Data.EnemyDiceResult);
    }

    [Fact]
    public void MissingRuleReturnsGameplayFailure()
    {
        var engine = new InitiativeEngine(new FixedDiceEngine(1));

        var result = engine.DetermineFirstTurn(CreateBattleContext());

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.MissingCombatRule, result.ErrorCode);
    }

    private static BattleContext CreateBattleContext()
    {
        return new BattleContext(
            new PlayerCharacter { Dexterity = 5 },
            new Enemy { Defense = 10 },
            new BattleState());
    }

    private sealed class FixedDiceEngine : IDiceEngine
    {
        private readonly int _result;

        public FixedDiceEngine(int result)
        {
            _result = result;
        }

        public EngineResult<DiceResult> Roll(DiceType dice)
        {
            return EngineResult<DiceResult>.Ok(new DiceResult(dice, _result, 0));
        }

        public EngineResult<DiceResult> RollWithModifier(DiceType dice, int modifier)
        {
            return EngineResult<DiceResult>.Ok(
                new DiceResult(dice, _result, modifier));
        }
    }

    private sealed class DeterministicInitiativeRule : IInitiativeRule
    {
        public EngineResult<InitiativeScores> CalculateScores(
            BattleContext battleContext,
            IDiceEngine diceEngine)
        {
            var playerDice = diceEngine.Roll(DiceType.D20).Data!.BaseRoll;
            var enemyDice = diceEngine.Roll(DiceType.D20).Data!.BaseRoll;

            return EngineResult<InitiativeScores>.Ok(
                new InitiativeScores(
                    battleContext.Player.Dexterity + playerDice,
                    battleContext.Enemy.Defense + enemyDice,
                    playerDice,
                    enemyDice));
        }
    }
}
