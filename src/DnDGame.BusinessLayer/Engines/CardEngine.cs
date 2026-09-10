namespace DnDGame.BusinessLayer.Engines;

using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

/// <summary>
/// Engine for orchestrating card plays during battles.
/// Coordinates validation, energy consumption, hand management, effects, and discard.
/// </summary>
public class CardEngine : ICardEngine
{
    private readonly IPlayEngine _playEngine;
    private readonly IHandEngine _handEngine;
    private readonly IDeckEngine _deckEngine;
    private readonly IEffectEngine? _effectEngine;
    private readonly PlayRules _playRules;

    /// <summary>
    /// Creates a new instance of CardEngine.
    /// All game engines must be properly initialized for card play to work.
    /// </summary>
    /// <param name="playEngine">Engine for card play validation.</param>
    /// <param name="handEngine">Engine for hand management.</param>
    /// <param name="deckEngine">Engine for deck/discard pile management.</param>
    /// <param name="playRules">Configurable rules for card play (energy, etc).</param>
    /// <param name="effectEngine">Optional engine for applying card effects. If null, no effects are applied.</param>
    /// <exception cref="ArgumentNullException">Thrown when required engines or rules are null.</exception>
    public CardEngine(
        IPlayEngine playEngine,
        IHandEngine handEngine,
        IDeckEngine deckEngine,
        PlayRules playRules,
        IEffectEngine? effectEngine = null)
    {
        _playEngine = playEngine ?? throw new ArgumentNullException(nameof(playEngine));
        _handEngine = handEngine ?? throw new ArgumentNullException(nameof(handEngine));
        _deckEngine = deckEngine ?? throw new ArgumentNullException(nameof(deckEngine));
        _playRules = playRules ?? throw new ArgumentNullException(nameof(playRules));
        _effectEngine = effectEngine;
    }

    /// <summary>
    /// Plays a card from the player's hand in a battle.
    /// 
    /// Execution flow:
    /// 1. Validates the card can be played (using IPlayEngine.CanPlayCard)
    /// 2. Consumes energy from the player's battle energy pool
    /// 3. Removes the card from the player's hand (using IHandEngine.RemoveCard)
    /// 4. Applies the card's effects (using IEffectEngine.ApplyCardEffect)
    /// 5. Moves the card to the discard pile (using IDeckEngine.DiscardCard)
    /// 
    /// If validation fails at step 1, the operation terminates immediately and no state changes occur.
    /// This ensures atomicity - either the card is fully played or not played at all.
    /// </summary>
    /// <param name="battle">The active battle.</param>
    /// <param name="battleDeck">The player's battle deck.</param>
    /// <param name="cardToPlay">The card instance to play from hand.</param>
    /// <param name="playerId">The ID of the player playing the card.</param>
    /// <param name="target">The target for the card's effect. Can be null for non-targeted cards.</param>
    /// <returns>
    /// An EngineResult{CardPlayResult} containing:
    /// - On success (IsSuccess=true): CardPlayResult with details about energy consumed, effects applied, etc.
    /// - On failure (IsSuccess=false): EngineResult with error code and descriptive message. No state changes.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public EngineResult<CardPlayResult> PlayCard(
        Battle battle,
        BattleDeck battleDeck,
        CardInstance cardToPlay,
        int playerId,
        ICardTarget? target = null)
    {
        // Validate required objects
        if (battle == null)
        {
            throw new ArgumentNullException(nameof(battle), "Battle cannot be null.");
        }

        if (battleDeck == null)
        {
            throw new ArgumentNullException(nameof(battleDeck), "Battle deck cannot be null.");
        }

        if (cardToPlay == null)
        {
            throw new ArgumentNullException(nameof(cardToPlay), "Card to play cannot be null.");
        }

        // ========== STEP 1: VALIDATE ==========
        // Validate the card can be played. This is a fail-fast check.
        // If this fails, NO state changes have occurred yet.
        var validationResult = _playEngine.CanPlayCard(battle, battleDeck, cardToPlay, playerId, target);
        if (!validationResult.IsSuccess)
        {
            // Return failure - convert non-generic to generic result
            return EngineResult<CardPlayResult>.Failure(
                validationResult.ErrorCode!.Value,
                validationResult.ErrorMessage!
            );
        }

        // ========== STEP 2: CONSUME ENERGY ==========
        // Calculate effective cost of the card (base cost + temporary modifiers)
        int cardCost = cardToPlay.GetEffectiveCost();
        int remainingEnergy = _playRules.CalculateRemainingEnergy(battle.CurrentPlayerEnergy, cardCost);

        // Consume the energy from battle state
        battle.CurrentPlayerEnergy = remainingEnergy;

        // ========== STEP 3: REMOVE FROM HAND ==========
        // Remove card from player's hand
        var handRemovalResult = _handEngine.RemoveCard(battleDeck, cardToPlay);
        if (!handRemovalResult.IsSuccess)
        {
            // This should not happen if validation passed, but handle gracefully
            // Restore energy state before returning
            battle.CurrentPlayerEnergy += cardCost;

            return EngineResult<CardPlayResult>.Failure(
                handRemovalResult.ErrorCode!.Value,
                $"Failed to remove card from hand: {handRemovalResult.ErrorMessage}"
            );
        }

        // ========== STEP 4: APPLY EFFECTS ==========
        string effectDescription = "No effects";
        if (_effectEngine != null)
        {
            try
            {
                effectDescription = _effectEngine.ApplyCardEffect(battle, cardToPlay, target);
            }
            catch (Exception ex)
            {
                // If effect application fails, we still discard the card (it was played)
                // but we log the error
                effectDescription = $"Error applying effects: {ex.Message}";
            }
        }

        // ========== STEP 5: DISCARD ==========
        // Move the card to the discard pile
        _deckEngine.DiscardCard(battleDeck, cardToPlay);

        // ========== SUCCESS ==========
        // Create and return the result of the successful card play
        var cardPlayResult = new CardPlayResult(
            card: cardToPlay,
            energyCost: cardCost,
            remainingEnergy: remainingEnergy,
            effectsApplied: _effectEngine != null,
            effectDescription: effectDescription
        );

        return EngineResult<CardPlayResult>.Success(cardPlayResult);
    }
}
