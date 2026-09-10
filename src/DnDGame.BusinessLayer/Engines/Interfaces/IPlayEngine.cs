namespace DnDGame.BusinessLayer.Engines.Interfaces;

using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;

/// <summary>
/// Represents a potential target for a card effect.
/// Can be an enemy, player character, or a location/environment element.
/// </summary>
public interface ICardTarget
{
    /// <summary>
    /// The ID of the target entity.
    /// </summary>
    int TargetId { get; }

    /// <summary>
    /// The type of target (Enemy, PlayerCharacter, Environmental, etc.).
    /// </summary>
    string TargetType { get; }
}

/// <summary>
/// Interface for the play engine that validates and manages card plays during battles.
/// Handles validation logic for card plays without applying effects.
/// </summary>
public interface IPlayEngine
{
    /// <summary>
    /// Validates whether a card can be played from the player's hand.
    /// Checks:
    /// - Battle is active
    /// - It's the player's turn
    /// - Card is in player's hand
    /// - Player has sufficient energy
    /// - Target is valid for the card
    ///
    /// Does NOT modify battle state or remove the card from hand if validation fails.
    /// </summary>
    /// <param name="battle">The active battle.</param>
    /// <param name="battleDeck">The player's battle deck.</param>
    /// <param name="cardToPlay">The card instance to play.</param>
    /// <param name="playerId">The ID of the player attempting to play the card.</param>
    /// <param name="target">The intended target for the card effect. Can be null for non-targeted cards.</param>
    /// <returns>An EngineResult indicating if the card can be played, with error codes for failures.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    EngineResult CanPlayCard(Battle battle, BattleDeck battleDeck, CardInstance cardToPlay, int playerId, ICardTarget? target = null);

    /// <summary>
    /// Checks if a specific battle is in an active, playable state.
    /// </summary>
    /// <param name="battle">The battle to check.</param>
    /// <returns>True if the battle is active and playable, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battle is null.</exception>
    bool IsBattleActive(Battle battle);

    /// <summary>
    /// Checks if it's the specified player's turn in the battle.
    /// </summary>
    /// <param name="battle">The active battle.</param>
    /// <param name="playerId">The ID of the player to check.</param>
    /// <returns>True if it's the player's turn, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battle is null.</exception>
    bool IsPlayerTurn(Battle battle, int playerId);

    /// <summary>
    /// Validates that the target is valid for a specific card type.
    /// </summary>
    /// <param name="cardToPlay">The card being played.</param>
    /// <param name="target">The target for the card. Can be null for non-targeted cards.</param>
    /// <returns>True if the target is valid for the card, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when cardToPlay is null.</exception>
    bool IsValidTarget(CardInstance cardToPlay, ICardTarget? target);
}
