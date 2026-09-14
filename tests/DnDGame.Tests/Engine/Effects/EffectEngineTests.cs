using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Effects;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Models;

namespace DnDGame.Tests.Engine.Effects;

public class EffectEngineTests
{
    private readonly EffectEngine _effectEngine = new();

    [Fact]
    public void PoisonDealsDamageAtTheStartOfThePoisonedCombatantsTurn()
    {
        var battleState = StateWith(new PoisonEffect(value: 2, duration: 3, target: EffectTarget.Enemy, stackCount: 2));

        _effectEngine.ApplyStartOfTurnEffects(battleState, TurnType.Enemy);

        Assert.Equal(6, battleState.EnemyHealth); // 10 - (2 * 2)
    }

    [Fact]
    public void BurnDealsDamageAtTheStartOfTheBurnedCombatantsTurn()
    {
        var battleState = StateWith(new BurnEffect(value: 3, duration: 1, target: EffectTarget.Player));

        _effectEngine.ApplyStartOfTurnEffects(battleState, TurnType.Player);

        Assert.Equal(7, battleState.PlayerHealth); // 10 - 3
    }

    [Fact]
    public void DamageOverTimeConsumesDurationAndIsRemovedWhenExpired()
    {
        var effect = new BurnEffect(value: 1, duration: 1, target: EffectTarget.Enemy);
        var battleState = StateWith(effect);

        _effectEngine.ApplyStartOfTurnEffects(battleState, TurnType.Enemy);

        Assert.True(effect.IsExpired);
        Assert.DoesNotContain(effect, battleState.ActiveEffects);
    }

    [Fact]
    public void DamageOverTimeDoesNotTickAtTheEndOfTurn()
    {
        var effect = new BurnEffect(value: 5, duration: 2, target: EffectTarget.Player);
        var battleState = StateWith(effect);

        _effectEngine.ApplyEndOfTurnEffects(battleState, TurnType.Player);

        Assert.Equal(10, battleState.PlayerHealth);
        Assert.Equal(2, effect.Duration);
    }

    [Fact]
    public void DamageOverTimeOnlyTicksItsOwnCombatant()
    {
        var battleState = StateWith(new BurnEffect(value: 5, duration: 2, target: EffectTarget.Player));

        _effectEngine.ApplyStartOfTurnEffects(battleState, TurnType.Enemy);

        Assert.Equal(10, battleState.PlayerHealth);
    }

    [Fact]
    public void StatModifierTicksAtTheEndOfTheAffectedCombatantsTurn()
    {
        var effect = new StrengthEffect(value: 3, duration: 2, target: EffectTarget.Player);
        var battleState = StateWith(effect);

        _effectEngine.ApplyEndOfTurnEffects(battleState, TurnType.Player);

        Assert.Equal(1, effect.Duration);
        Assert.Contains(effect, battleState.ActiveEffects);
    }

    [Fact]
    public void StatModifierIsRemovedWhenExpired()
    {
        var effect = new StrengthEffect(value: 3, duration: 1, target: EffectTarget.Player);
        var battleState = StateWith(effect);

        _effectEngine.ApplyEndOfTurnEffects(battleState, TurnType.Player);

        Assert.DoesNotContain(effect, battleState.ActiveEffects);
    }

    [Fact]
    public void StatModifierDoesNotTickAtTheStartOfTurn()
    {
        var effect = new WeakEffect(value: 2, duration: 2, target: EffectTarget.Enemy);
        var battleState = StateWith(effect);

        _effectEngine.ApplyStartOfTurnEffects(battleState, TurnType.Enemy);

        Assert.Equal(2, effect.Duration);
    }

    [Fact]
    public void StatModifierOnlyTicksItsOwnCombatant()
    {
        var effect = new DefenseUpEffect(value: 2, duration: 2, target: EffectTarget.Player);
        var battleState = StateWith(effect);

        _effectEngine.ApplyEndOfTurnEffects(battleState, TurnType.Enemy);

        Assert.Equal(2, effect.Duration);
    }

    [Fact]
    public void HealthIsClampedAtZero()
    {
        var battleState = new BattleState
        {
            PlayerHealth = 2,
            PlayerMaxHealth = 10,
            EnemyHealth = 10,
            EnemyMaxHealth = 10,
            ActiveEffects = new List<ActiveEffect>
            {
                new BurnEffect(value: 6, duration: 1, target: EffectTarget.Player, stackCount: 2)
            }
        };

        _effectEngine.ApplyStartOfTurnEffects(battleState, TurnType.Player);

        Assert.Equal(0, battleState.PlayerHealth); // 2 - 12 clamped
    }

    [Fact]
    public void TickIsWrittenToTheBattleLog()
    {
        var battleState = StateWith(new BurnEffect(value: 2, duration: 1, target: EffectTarget.Player));
        battleState.TurnNumber = 3;

        _effectEngine.ApplyStartOfTurnEffects(battleState, TurnType.Player);

        var entry = Assert.Single(battleState.BattleLog);
        Assert.Equal("Burn", entry.Actor);
        Assert.Equal("Tick", entry.Action);
        Assert.Equal(2, entry.Damage);
        Assert.Equal(3, entry.TurnNumber);
    }

    private static BattleState StateWith(ActiveEffect effect) => new()
    {
        PlayerHealth = 10,
        PlayerMaxHealth = 10,
        EnemyHealth = 10,
        EnemyMaxHealth = 10,
        ActiveEffects = new List<ActiveEffect> { effect }
    };
}