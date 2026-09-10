namespace DnDGame.BusinessLayer.Effects.Strategies;

using DnDGame.BusinessLayer.Effects.Interfaces;

/// <summary>
/// Effect that deals damage to a target.
/// Demonstrates a basic concrete effect strategy.
/// </summary>
public class DamageEffect : ICardEffect
{
    public string EffectName => "Damage";

    /// <summary>
    /// Damage effect can apply if:
    /// - Card has damage value
    /// - Card has a valid target (if required)
    /// </summary>
    public bool CanApply(CardEffectContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Can't apply damage if card has no damage value
        if (context.EffectiveCardDamage <= 0)
        {
            return false;
        }

        // For single-target damage, must have a target
        if (context.CardDefinition.TargetType == Domain.Enums.TargetType.SingleEnemy ||
            context.CardDefinition.TargetType == Domain.Enums.TargetType.SingleAlly)
        {
            return context.HasTarget && context.Target!.TargetId > 0;
        }

        return true;
    }

    /// <summary>
    /// Applies damage to the target(s).
    /// For now, just returns a description (damage logic would be in DamageCalculator).
    /// </summary>
    public string Apply(CardEffectContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (!CanApply(context))
        {
            return $"Cannot apply {EffectName} effect.";
        }

        int damage = context.EffectiveCardDamage;

        // In a full implementation, this would call a DamageCalculator
        // For now, just return a description
        if (context.TargetsArea)
        {
            return $"Dealt {damage} damage to all enemies.";
        }
        else if (context.HasTarget)
        {
            return $"Dealt {damage} damage to target ID {context.Target!.TargetId}.";
        }
        else
        {
            return $"Dealt {damage} damage.";
        }
    }
}
