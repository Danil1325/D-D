namespace DnDGame.BusinessLayer.Effects.Strategies;

using DnDGame.BusinessLayer.Effects.Interfaces;

/// <summary>
/// Effect that heals a target.
/// Demonstrates another concrete effect strategy.
/// </summary>
public class HealEffect : ICardEffect
{
    public string EffectName => "Heal";

    /// <summary>
    /// Heal effect can apply if:
    /// - Card has healing value (represented by BaseDamage for now)
    /// - Target exists (healing is typically targeted)
    /// </summary>
    public bool CanApply(CardEffectContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Can't heal if card has no healing value
        if (context.EffectiveCardDamage <= 0)
        {
            return false;
        }

        // Heal typically targets self or an ally
        if (context.TargetsSelf)
        {
            return true;
        }

        // If targeting an ally, must have a valid target
        if (context.CardDefinition.TargetType == Domain.Enums.TargetType.SingleAlly ||
            context.CardDefinition.TargetType == Domain.Enums.TargetType.AllAllies)
        {
            return context.HasTarget && context.Target!.TargetId > 0;
        }

        return false;
    }

    /// <summary>
    /// Applies healing to the target(s).
    /// For now, just returns a description (healing logic would be in a health manager).
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

        int healAmount = context.EffectiveCardDamage;

        // In a full implementation, this would update character health
        // For now, just return a description
        if (context.TargetsSelf)
        {
            return $"Healed self for {healAmount} HP.";
        }
        else if (context.TargetsArea)
        {
            return $"Healed all allies for {healAmount} HP each.";
        }
        else if (context.HasTarget)
        {
            return $"Healed target ID {context.Target!.TargetId} for {healAmount} HP.";
        }
        else
        {
            return $"Healed for {healAmount} HP.";
        }
    }
}
