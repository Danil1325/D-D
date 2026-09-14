using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Models;

namespace DnDGame.Domain.Engine.Effects;

/// <summary>
/// Resolves effects that persist across turn boundaries.
/// Damage-over-time effects (Burn, Poison) deal their stacked value at the start
/// of the affected combatant's own turn; temporary stat modifiers (Strength,
/// Weak, Vulnerable, DefenseUp) only tick down at the end of the affected
/// combatant's turn. Every resolution consumes one unit of the effect's duration.
/// </summary>
public sealed class EffectEngine : IEffectEngine
{
    private static readonly string[] DamageOverTimeTypes = [BurnEffect.EffectType, PoisonEffect.EffectType];

    public void ApplyStartOfTurnEffects(BattleState battleState, TurnType turn)
    {
        ArgumentNullException.ThrowIfNull(battleState);

        var target = ToEffectTarget(turn);

        foreach (var effect in battleState.ActiveEffects
            .Where(e => IsDamageOverTime(e) && e.Target == target)
            .ToList())
        {
            var damage = effect.Value * effect.StackCount;
            DealDamage(battleState, turn, damage);

            battleState.BattleLog.Add(new BattleLogEntry(
                battleState.TurnNumber,
                actor: effect.Type,
                action: "Tick",
                damage: damage,
                result: $"{effect.Type} dealt {damage} damage to the {(turn == TurnType.Player ? "player" : "enemy")}."));

            effect.ConsumeDuration();
        }

        RemoveExpired(battleState, turn);
    }

    public void ApplyEndOfTurnEffects(BattleState battleState, TurnType turn)
    {
        ArgumentNullException.ThrowIfNull(battleState);

        var target = ToEffectTarget(turn);

        foreach (var effect in battleState.ActiveEffects
            .Where(e => !IsDamageOverTime(e) && e.Target == target)
            .ToList())
        {
            effect.ConsumeDuration();
        }

        RemoveExpired(battleState, turn);
    }

    private static EffectTarget ToEffectTarget(TurnType turn)
    {
        return turn == TurnType.Player ? EffectTarget.Player : EffectTarget.Enemy;
    }

    private static bool IsDamageOverTime(ActiveEffect effect)
    {
        return DamageOverTimeTypes.Contains(effect.Type);
    }

    private static void DealDamage(BattleState battleState, TurnType turn, int damage)
    {
        if (turn == TurnType.Player)
        {
            battleState.PlayerHealth = Math.Max(0, battleState.PlayerHealth - damage);
        }
        else
        {
            battleState.EnemyHealth = Math.Max(0, battleState.EnemyHealth - damage);
        }
    }

    private static void RemoveExpired(BattleState battleState, TurnType turn)
    {
        var target = ToEffectTarget(turn);

        foreach (var expired in battleState.ActiveEffects.Where(e => e.Target == target && e.IsExpired).ToList())
        {
            battleState.ActiveEffects.Remove(expired);
        }
    }
}