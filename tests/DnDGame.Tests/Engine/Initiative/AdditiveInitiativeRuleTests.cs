using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;
using DnDGame.Domain.Engine.Initiative;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Enemies;
using Xunit;

namespace DnDGame.Tests.Engine.Initiative;

public class AdditiveInitiativeRuleTests
{
    [Fact]
    public void CalculateScores_AddsDexterityAndDefenseToEachSidesD20Roll()
    {
        var rule = new AdditiveInitiativeRule();
        var battleContext = new BattleContext(
            new PlayerCharacter { Dexterity = 5 },
            new Enemy { Defense = 10 },
            new BattleState());

        var result = rule.CalculateScores(battleContext, new SequentialDiceEngine(4, 6));

        Assert.True(result.Success);
        Assert.Equal(9, result.Data!.PlayerScore);
        Assert.Equal(16, result.Data.EnemyScore);
        Assert.Equal(4, result.Data.PlayerDiceResult);
        Assert.Equal(6, result.Data.EnemyDiceResult);
    }

    [Fact]
    public void CalculateScores_PropagatesDiceEngineFailure()
    {
        var rule = new AdditiveInitiativeRule();
        var battleContext = new BattleContext(
            new PlayerCharacter { Dexterity = 5 },
            new Enemy { Defense = 10 },
            new BattleState());

        var result = rule.CalculateScores(battleContext, new FailingDiceEngine());

        Assert.False(result.Success);
    }

    private sealed class SequentialDiceEngine : IDiceEngine
    {
        private readonly Queue<int> _rolls;

        public SequentialDiceEngine(params int[] rolls)
        {
            _rolls = new Queue<int>(rolls);
        }

        public EngineResult<DiceResult> Roll(DiceType dice) =>
            EngineResult<DiceResult>.Ok(new DiceResult(dice, _rolls.Dequeue(), 0));

        public EngineResult<DiceResult> RollWithModifier(DiceType dice, int modifier) =>
            EngineResult<DiceResult>.Ok(new DiceResult(dice, _rolls.Dequeue(), modifier));
    }

    private sealed class FailingDiceEngine : IDiceEngine
    {
        public EngineResult<DiceResult> Roll(DiceType dice) =>
            EngineResult<DiceResult>.Fail("Dice engine unavailable.", EngineErrorCodes.InvalidDice);

        public EngineResult<DiceResult> RollWithModifier(DiceType dice, int modifier) =>
            EngineResult<DiceResult>.Fail("Dice engine unavailable.", EngineErrorCodes.InvalidDice);
    }
}
