namespace DnDGame.BusinessLayer.Engines;

using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

/// <summary>
/// Engine for managing card plays during battles.
/// Validates card plays without applying effects or modifying game state on failure.
/// </summary>
public class PlayEngine : IPlayEngine
{
    private readonly PlayRules _playRules;

    /// <summary>
    /// Creates a new instance of PlayEngine with the specified play rules.
    /// </summary>
    /// <param name="playRules">The configurable rules for card play.</param>
    /// <exception cref="ArgumentNullException">Thrown when playRules is null.</exception>
    public PlayEngine(PlayRules playRules)
    {
        _playRules = playRules ?? throw new ArgumentNullException(nameof(playRules));
    }

    /// <summary>
    /// Validates whether a card can be played from the player's hand.
    /// Performs comprehensive validation without modifying game state on failure:
    /// 1. Validates all required objects are not null
    /// 2. Checks if battle is in active state
    /// 3. Checks if it's the player's turn
    /// 4. Verifies card is in player's hand
    /// 5. Verifies player has sufficient energy to pay the card cost
    /// 6. Validates the target is appropriate for the card type
    /// </summary>
    /// <param name="battle">The active battle.</param>
    /// <param name="battleDeck">The player's battle deck.</param>
    /// <param name="cardToPlay">The card instance to play.</param>
    /// <param name="playerId">The ID of the player attempting to play the card.</param>
    /// <param name="target">The intended target for the card effect. Can be null for non-targeted cards.</param>
    /// <returns>
    /// EngineResult.Success() if all validations pass.
    /// EngineResult.Failure() with error codes:
    /// - BATTLE_NOT_ACTIVE: Battle is not active
    /// - NOT_PLAYER_TURN: It's not the player's turn
    /// - CARD_NOT_IN_HAND: Card is not in player's hand
    /// - INSUFFICIENT_ENERGY: Player lacks energy to play the card
    /// - INVALID_TARGET: Target is invalid for this card
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when battle, battleDeck, or cardToPlay is null.</exception>
    public EngineResult CanPlayCard(Battle battle, BattleDeck battleDeck, CardInstance cardToPlay, int playerId, ICardTarget? target = null)
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

        // Check if battle is active
        if (!IsBattleActive(battle))
        {
            return EngineResult.Failure(
                ErrorCode.BATTLE_NOT_ACTIVE,
                "Battle is not currently active or is not in a playable state."
            );
        }

        // Check if it's the player's turn
        if (!IsPlayerTurn(battle, playerId))
        {
            return EngineResult.Failure(
                ErrorCode.NOT_PLAYER_TURN,
                $"It is not player {playerId}'s turn. Current turn is for player {battle.CurrentPlayerTurnId}."
            );
        }

        // Check if card is in the player's hand
        if (!battleDeck.Hand.Contains(cardToPlay))
        {
            return EngineResult.Failure(
                ErrorCode.CARD_NOT_IN_HAND,
                $"Card with instance ID '{cardToPlay.InstanceId}' is not in the player's hand."
            );
        }

        // Get the card definition to check cost and type
        var cardDefinition = cardToPlay.Card;
        if (cardDefinition == null)
        {
            return EngineResult.Failure(
                ErrorCode.CARD_CANNOT_BE_PLAYED,
                $"Card definition for instance '{cardToPlay.InstanceId}' is missing or corrupted."
            );
        }

        // Get effective card cost (base cost + temporary modifiers)
        int cardCost = cardToPlay.GetEffectiveCost();

        // Check if player has sufficient energy
        if (!_playRules.CanAffordCard(battle.CurrentPlayerEnergy, cardCost))
        {
            return EngineResult.Failure(
                ErrorCode.INSUFFICIENT_ENERGY,
                $"Insufficient energy to play card '{cardDefinition.Name}'. " +
                $"Card cost: {cardCost}, Current energy: {battle.CurrentPlayerEnergy}."
            );
        }

        // Check if target is valid for the card
        if (!IsValidTarget(cardToPlay, target))
        {
            return EngineResult.Failure(
                ErrorCode.INVALID_TARGET,
                $"The specified target is invalid for card '{cardDefinition.Name}'. " +
                $"Card targets: {cardDefinition.TargetType}."
            );
        }

        // All validations passed
        return EngineResult.Success();
    }

    /// <summary>
    /// Checks if a specific battle is in an active, playable state.
    /// A battle is considered active if:
    /// - Battle exists and is not null
    /// - Battle status is InProgress
    /// - Battle has not been completed
    /// </summary>
    /// <param name="battle">The battle to check.</param>
    /// <returns>True if the battle is active and playable, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battle is null.</exception>
    public bool IsBattleActive(Battle battle)
    {
        if (battle == null)
        {
            throw new ArgumentNullException(nameof(battle), "Battle cannot be null.");
        }

        // Battle is active if status is InProgress and CompletedAt is null
        return battle.Status == GameSessionStatus.InProgress && battle.CompletedAt == null;
    }

    /// <summary>
    /// Checks if it's the specified player's turn in the battle.
    /// </summary>
    /// <param name="battle">The active battle.</param>
    /// <param name="playerId">The ID of the player to check.</param>
    /// <returns>True if it's the player's turn, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battle is null.</exception>
    public bool IsPlayerTurn(Battle battle, int playerId)
    {
        if (battle == null)
        {
            throw new ArgumentNullException(nameof(battle), "Battle cannot be null.");
        }

        return battle.CurrentPlayerTurnId == playerId;
    }

    /// <summary>
    /// Validates that the target is valid for a specific card type.
    /// Rules:
    /// - If card has TargetType.Self, target must be null (self effect)
    /// - If card has TargetType.None, target must be null
    /// - If card requires a target (SingleEnemy, SingleAlly, RandomTarget), a valid target must be provided
    /// - If card affects multiple targets (AllEnemies, AllAllies), target selection is not needed
    /// </summary>
    /// <param name="cardToPlay">The card being played.</param>
    /// <param name="target">The target for the card. Can be null for non-targeted cards.</param>
    /// <returns>True if the target is valid for the card, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when cardToPlay is null.</exception>
    public bool IsValidTarget(CardInstance cardToPlay, ICardTarget? target)
    {
        if (cardToPlay == null)
        {
            throw new ArgumentNullException(nameof(cardToPlay), "Card to play cannot be null.");
        }

        var cardDefinition = cardToPlay.Card;
        if (cardDefinition == null)
        {
            return false;
        }

        // Get target type from card definition
        var cardTargetType = cardDefinition.TargetType;

        // Handle different target types
        switch (cardTargetType)
        {
            case TargetType.None:
                // Non-targeted cards should not have a target
                return target == null;

            case TargetType.Self:
                // Self-targeted cards don't require a specific external target
                return true;

            case TargetType.SingleEnemy:
            case TargetType.SingleAlly:
                // These cards require a single valid target
                if (target == null)
                {
                    return false;
                }
                // Target must not be empty/default
                return target.TargetId > 0;

            case TargetType.RandomTarget:
                // Random target cards don't need specific target selection
                // The engine will choose randomly when the card is played
                return true;

            case TargetType.AllEnemies:
            case TargetType.AllAllies:
                // Area-effect cards don't require specific target selection
                return true;

            default:
                // Unknown target type - reject to be safe
                return false;
        }
    }
}
