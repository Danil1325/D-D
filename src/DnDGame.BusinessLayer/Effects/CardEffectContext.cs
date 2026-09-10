namespace DnDGame.BusinessLayer.Effects;

using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

/// <summary>
/// Provides context and state information to card effects during execution.
/// Contains all battle information needed for effects to apply their logic.
/// </summary>
public class CardEffectContext
{
    /// <summary>
    /// The active battle where the effect is being applied.
    /// </summary>
    public Battle Battle { get; set; }

    /// <summary>
    /// The card instance that triggered the effect.
    /// Contains both card definition and runtime modifiers.
    /// </summary>
    public CardInstance PlayedCard { get; set; }

    /// <summary>
    /// The battle deck of the player who played the card.
    /// </summary>
    public BattleDeck PlayerBattleDeck { get; set; }

    /// <summary>
    /// The ID of the player who played the card.
    /// </summary>
    public int PlayerId { get; set; }

    /// <summary>
    /// The target of the effect. Can be null for area effects or self effects.
    /// </summary>
    public ICardTarget? Target { get; set; }

    /// <summary>
    /// The card definition from the played card.
    /// Shorthand for PlayedCard.Card.
    /// </summary>
    public Card CardDefinition { get; set; }

    /// <summary>
    /// Optional hand engine for effects that need to draw/manage hand.
    /// </summary>
    public IHandEngine? HandEngine { get; set; }

    /// <summary>
    /// Optional deck engine for effects that need to interact with deck.
    /// </summary>
    public IDeckEngine? DeckEngine { get; set; }

    /// <summary>
    /// Optional hand rules for effects that need to check hand limits.
    /// </summary>
    public HandRules? HandRules { get; set; }

    /// <summary>
    /// Creates a new CardEffectContext with all required battle information.
    /// </summary>
    /// <param name="battle">The active battle.</param>
    /// <param name="playedCard">The card instance being played.</param>
    /// <param name="playerBattleDeck">The player's battle deck.</param>
    /// <param name="playerId">The player ID.</param>
    /// <param name="cardDefinition">The card definition.</param>
    /// <param name="target">Optional target for the effect.</param>
    /// <param name="handEngine">Optional hand engine for draw effects.</param>
    /// <param name="deckEngine">Optional deck engine for card interactions.</param>
    /// <param name="handRules">Optional hand rules for size checking.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public CardEffectContext(
        Battle battle,
        CardInstance playedCard,
        BattleDeck playerBattleDeck,
        int playerId,
        Card cardDefinition,
        ICardTarget? target = null,
        IHandEngine? handEngine = null,
        IDeckEngine? deckEngine = null,
        HandRules? handRules = null)
    {
        Battle = battle ?? throw new ArgumentNullException(nameof(battle));
        PlayedCard = playedCard ?? throw new ArgumentNullException(nameof(playedCard));
        PlayerBattleDeck = playerBattleDeck ?? throw new ArgumentNullException(nameof(playerBattleDeck));
        CardDefinition = cardDefinition ?? throw new ArgumentNullException(nameof(cardDefinition));
        PlayerId = playerId;
        Target = target;
        HandEngine = handEngine;
        DeckEngine = deckEngine;
        HandRules = handRules;
    }

    /// <summary>
    /// Gets the effective cost of the played card (base cost + modifiers).
    /// </summary>
    public int EffectiveCardCost => PlayedCard.GetEffectiveCost();

    /// <summary>
    /// Gets the effective damage of the played card (base damage + modifiers).
    /// </summary>
    public int EffectiveCardDamage => PlayedCard.GetEffectiveDamage();

    /// <summary>
    /// Determines if the effect has a target.
    /// </summary>
    public bool HasTarget => Target != null;

    /// <summary>
    /// Determines if the card targets self.
    /// </summary>
    public bool TargetsSelf => CardDefinition.TargetType == TargetType.Self;

    /// <summary>
    /// Determines if the card targets area (multiple combatants).
    /// </summary>
    public bool TargetsArea => CardDefinition.TargetType == TargetType.AllEnemies ||
                               CardDefinition.TargetType == TargetType.AllAllies;
}
