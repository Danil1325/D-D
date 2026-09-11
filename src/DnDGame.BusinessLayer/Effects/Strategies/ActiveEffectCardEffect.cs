using DnDGame.BusinessLayer.Effects.Interfaces;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Models;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Effects.Strategies;

/// <summary>
/// Shared card strategy for effects that are stored and resolved at future turn
/// boundaries. Concrete strategies provide only their effect factory.
/// </summary>
public abstract class ActiveEffectCardEffect<TActiveEffect> : ICardEffect
    where TActiveEffect : ActiveEffect
{
    private readonly Func<int, int, EffectTarget, TActiveEffect> _effectFactory;

    protected ActiveEffectCardEffect(
        string effectName,
        int duration,
        Func<int, int, EffectTarget, TActiveEffect> effectFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(effectName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(duration);

        EffectName = effectName;
        Duration = duration;
        _effectFactory = effectFactory ?? throw new ArgumentNullException(nameof(effectFactory));
    }

    public string EffectName { get; }

    protected int Duration { get; }

    public bool CanApply(CardEffectContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.EffectiveCardDamage <= 0 || context.ActiveEffects is null)
        {
            return false;
        }

        return context.CardDefinition.TargetType switch
        {
            TargetType.Self or TargetType.AllAllies or TargetType.AllEnemies => true,
            TargetType.SingleAlly or TargetType.SingleEnemy => context.HasTarget && context.Target!.TargetId > 0,
            _ => false
        };
    }

    public string Apply(CardEffectContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!CanApply(context))
        {
            return $"Cannot apply {EffectName} effect.";
        }

        var effect = _effectFactory(
            context.EffectiveCardDamage,
            Duration,
            ResolveTarget(context.CardDefinition.TargetType));
        context.ActiveEffects!.Add(effect);

        return $"Applied {effect.Type} to {effect.Target} for {effect.Duration} turn(s).";
    }

    private static EffectTarget ResolveTarget(TargetType targetType) => targetType switch
    {
        TargetType.Self or TargetType.SingleAlly or TargetType.AllAllies => EffectTarget.Player,
        TargetType.SingleEnemy or TargetType.AllEnemies => EffectTarget.Enemy,
        _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, "A target is required for an active effect.")
    };
}
