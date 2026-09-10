namespace DnDGame.BusinessLayer.Engines;

using DnDGame.BusinessLayer.Effects;
using DnDGame.BusinessLayer.Effects.Interfaces;
using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;

/// <summary>
/// Effect engine implementation that applies card effects using the Strategy Pattern.
/// Uses CardEffectRegistry to lookup and execute effect strategies.
/// No switch statements or if-else chains - extensible by design.
/// </summary>
public class EffectEngine : IEffectEngine
{
    private readonly CardEffectRegistry _effectRegistry;

    /// <summary>
    /// Creates a new instance of EffectEngine with the specified effect registry.
    /// </summary>
    /// <param name="effectRegistry">The registry of available effect strategies.</param>
    /// <exception cref="ArgumentNullException">Thrown when effectRegistry is null.</exception>
    public EffectEngine(CardEffectRegistry effectRegistry)
    {
        _effectRegistry = effectRegistry ?? throw new ArgumentNullException(nameof(effectRegistry));
    }

    /// <summary>
    /// Applies the effects of a card using registered effect strategies.
    /// Creates a CardEffectContext and applies all relevant effects.
    /// Uses the Strategy Pattern - no large switch statements needed.
    /// </summary>
    /// <param name="battle">The active battle.</param>
    /// <param name="playedCard">The card instance being played.</param>
    /// <param name="battleDeck">The player's battle deck.</param>
    /// <param name="playerId">The ID of the player who played the card.</param>
    /// <param name="target">The target of the card's effect. Can be null for area effects or self effects.</param>
    /// <returns>A description of what the card effect did.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public string ApplyCardEffect(Battle battle, CardInstance playedCard, BattleDeck battleDeck, int playerId, ICardTarget? target = null)
    {
        if (battle == null)
        {
            throw new ArgumentNullException(nameof(battle), "Battle cannot be null.");
        }

        if (playedCard == null)
        {
            throw new ArgumentNullException(nameof(playedCard), "Played card cannot be null.");
        }

        if (battleDeck == null)
        {
            throw new ArgumentNullException(nameof(battleDeck), "Battle deck cannot be null.");
        }

        if (playedCard.Card == null)
        {
            return "Error: Card definition is missing.";
        }

        // Create effect context with all battle information
        var context = new CardEffectContext(
            battle: battle,
            playedCard: playedCard,
            playerBattleDeck: battleDeck,
            playerId: playerId,
            cardDefinition: playedCard.Card,
            target: target
        );

        // Get the primary effect type from the card's EffectType enum
        // (Assuming Card has an EffectType property that maps to effect strategy names)
        string effectTypeName = playedCard.Card.EffectType.ToString();

        // Lookup the effect strategy from registry
        var effect = _effectRegistry.GetEffect(effectTypeName);
        if (effect == null)
        {
            return $"No effect strategy registered for '{effectTypeName}'. Card has no effect.";
        }

        // Check if the effect can be applied in this context
        if (!effect.CanApply(context))
        {
            return $"Effect '{effectTypeName}' cannot be applied in the current battle context.";
        }

        // Apply the effect strategy
        try
        {
            string result = effect.Apply(context);
            return result;
        }
        catch (Exception ex)
        {
            return $"Error applying effect '{effectTypeName}': {ex.Message}";
        }
    }

    /// <summary>
    /// Calculates damage for a card against a specific target.
    /// Placeholder implementation - would be delegated to a DamageCalculator service.
    /// </summary>
    public int CalculateDamage(Battle battle, CardInstance card, int targetId)
    {
        if (card == null)
        {
            return 0;
        }

        // Placeholder: Just return the card's effective damage
        // In a real implementation, this would calculate based on:
        // - Card damage value
        // - Player stats/buffs
        // - Target defenses
        // - Battle modifiers
        return card.GetEffectiveDamage();
    }

    /// <summary>
    /// Applies status effects from a card to a target.
    /// Placeholder implementation - would be delegated to a StatusEffectEngine service.
    /// </summary>
    public string ApplyStatusEffect(Battle battle, CardInstance card, int targetId)
    {
        if (card == null)
        {
            return "No card provided.";
        }

        // Placeholder: Just return a description
        // In a real implementation, this would:
        // - Create status effect instances
        // - Apply them to the target
        // - Track duration/stacks
        return $"Applied status effects from card to target {targetId}.";
    }
}
