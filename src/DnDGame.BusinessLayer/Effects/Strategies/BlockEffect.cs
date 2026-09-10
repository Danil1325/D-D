namespace DnDGame.BusinessLayer.Effects.Strategies;

using DnDGame.BusinessLayer.Effects.Interfaces;

/// <summary>
/// Effect that applies block/shield to the player or target.
/// Block reduces incoming damage on the next attack.
/// </summary>
public class BlockEffect : ICardEffect
{
    public string EffectName => "Block";

    /// <summary>
    /// Block effect can apply if:
    /// - Card has a block value (using BaseDamage as block amount for now)
    /// - Effect targets self or is an area effect
    /// </summary>
    public bool CanApply(CardEffectContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Can't apply block if card has no value
        if (context.EffectiveCardDamage <= 0)
        {
            return false;
        }

        // Block typically targets self
        if (context.TargetsSelf)
        {
            return true;
        }

        // Block can target allies
        if (context.CardDefinition.TargetType == Domain.Enums.TargetType.SingleAlly ||
            context.CardDefinition.TargetType == Domain.Enums.TargetType.AllAllies)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Applies block to the player's battle deck.
    /// Block is cumulative and reduces incoming damage.
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

        int blockAmount = context.EffectiveCardDamage;
        int previousBlock = context.PlayerBattleDeck.CurrentBlock;

        // Add block to player's battle deck
        context.PlayerBattleDeck.CurrentBlock += blockAmount;

        if (context.TargetsSelf)
        {
            return $"Gained {blockAmount} block (total block: {context.PlayerBattleDeck.CurrentBlock}).";
        }
        else if (context.TargetsArea)
        {
            return $"All allies gained {blockAmount} block.";
        }
        else if (context.HasTarget)
        {
            return $"Target ID {context.Target!.TargetId} gained {blockAmount} block.";
        }
        else
        {
            return $"Gained {blockAmount} block.";
        }
    }
}
