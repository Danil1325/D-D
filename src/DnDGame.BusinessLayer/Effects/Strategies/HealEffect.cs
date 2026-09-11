namespace DnDGame.BusinessLayer.Effects.Strategies;

using DnDGame.BusinessLayer.Effects.Interfaces;

/// <summary>
/// Effect that heals a target or self.
/// Ensures healing never exceeds MaxHealth.
/// </summary>
public class HealEffect : ICardEffect
{
    public string EffectName => "Heal";

    /// <summary>
    /// Heal effect can apply if:
    /// - Card has healing value (represented by BaseDamage for now)
    /// - Target is valid (self or specified ally)
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
    /// Applies healing to the player.
    /// Healing is capped at MaxHealth and never exceeds it.
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
        int maxHealth = context.Battle.PlayerMaxHealth;
        int currentHealth = context.Battle.PlayerCurrentHealth;

        // Calculate actual healing (capped at MaxHealth)
        int actualHeal = Math.Min(healAmount, maxHealth - currentHealth);
        
        if (actualHeal <= 0)
        {
            return "Player is already at maximum health. No healing applied.";
        }

        // Apply heal
        context.Battle.PlayerCurrentHealth += actualHeal;

        if (context.TargetsSelf)
        {
            return $"Healed self for {actualHeal} HP (from {currentHealth} to {context.Battle.PlayerCurrentHealth}).";
        }
        else if (context.TargetsArea)
        {
            return $"Healed all allies for {actualHeal} HP each.";
        }
        else if (context.HasTarget)
        {
            return $"Healed target ID {context.Target!.TargetId} for {actualHeal} HP.";
        }
        else
        {
            return $"Healed for {actualHeal} HP (capped at MaxHealth).";
        }
    }
}
