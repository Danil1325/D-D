namespace DnDGame.BusinessLayer.Engines.Interfaces;

using DnDGame.BusinessLayer.Effects;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;

/// <summary>
/// Interface for the effect engine that applies card effects during battles.
/// Handles damage calculation, status effects, healing, and other card abilities.
/// This interface allows CardEngine to execute effects without depending on implementation details.
/// </summary>
public interface IEffectEngine
{
    /// <summary>
    /// Applies the effects of a card to the specified target(s) during a battle.
    /// Creates a CardEffectContext with all battle state and applies registered effect strategies.
    /// </summary>
    /// <param name="battle">The active battle.</param>
    /// <param name="playedCard">The card instance being played.</param>
    /// <param name="battleDeck">The player's battle deck.</param>
    /// <param name="playerId">The ID of the player who played the card.</param>
    /// <param name="target">The target of the card's effect. Can be null for area effects or self effects.</param>
    /// <returns>A description of what the card effect did.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battle, playedCard, or battleDeck is null.</exception>
    string ApplyCardEffect(Battle battle, CardInstance playedCard, BattleDeck battleDeck, int playerId, ICardTarget? target = null);

    /// <summary>
    /// Calculates damage for a card against a specific target.
    /// Takes into account card properties, target defenses, and battle modifiers.
    /// </summary>
    /// <param name="battle">The active battle.</param>
    /// <param name="card">The card dealing damage.</param>
    /// <param name="targetId">The ID of the target taking damage.</param>
    /// <returns>The calculated damage value.</returns>
    int CalculateDamage(Battle battle, CardInstance card, int targetId);

    /// <summary>
    /// Applies status effects from a card to a target (e.g., poison, stun, buff).
    /// </summary>
    /// <param name="battle">The active battle.</param>
    /// <param name="card">The card applying the status effect.</param>
    /// <param name="targetId">The ID of the target receiving the effect.</param>
    /// <returns>A description of the status effects applied.</returns>
    string ApplyStatusEffect(Battle battle, CardInstance card, int targetId);
}
