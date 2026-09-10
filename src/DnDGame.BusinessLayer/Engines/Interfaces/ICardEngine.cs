namespace DnDGame.BusinessLayer.Engines.Interfaces;

using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;

/// <summary>
/// Interface for the card engine that orchestrates card play during battles.
/// Coordinates validation, energy consumption, hand management, effects, and discard.
/// </summary>
public interface ICardEngine
{
    /// <summary>
    /// Plays a card from the player's hand in a battle.
    /// 
    /// Execution flow:
    /// 1. Validates the card can be played (using IPlayEngine)
    /// 2. Consumes energy from the player's battle energy pool
    /// 3. Removes the card from the player's hand (using IHandEngine)
    /// 4. Applies the card's effects (using IEffectEngine)
    /// 5. Moves the card to the discard pile (using IDeckEngine)
    /// 
    /// If validation fails at step 1, the operation terminates and no state changes occur.
    /// </summary>
    /// <param name="battle">The active battle.</param>
    /// <param name="battleDeck">The player's battle deck.</param>
    /// <param name="cardToPlay">The card instance to play from hand.</param>
    /// <param name="playerId">The ID of the player playing the card.</param>
    /// <param name="target">The target for the card's effect. Can be null for non-targeted cards.</param>
    /// <returns>
    /// An EngineResult{CardPlayResult} containing:
    /// - On success: CardPlayResult with details about energy consumed, effects applied, etc.
    /// - On failure: EngineResult with error code and descriptive message. No state changes.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    EngineResult<CardPlayResult> PlayCard(
        Battle battle,
        BattleDeck battleDeck,
        CardInstance cardToPlay,
        int playerId,
        ICardTarget? target = null
    );
}
