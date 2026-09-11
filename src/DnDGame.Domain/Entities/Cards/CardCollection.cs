using DnDGame.Domain.Common;
using DnDGame.Domain.Entities.Characters;

namespace DnDGame.Domain.Entities.Cards;

/// <summary>All cards owned or unlockable by one player character.</summary>
public class CardCollection : BaseEntity
{
    public CardCollection(int playerCharacterId)
    {
        if (playerCharacterId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(playerCharacterId), "Player character ID must be positive.");
        }

        PlayerCharacterId = playerCharacterId;
    }

    public int PlayerCharacterId { get; }

    public PlayerCharacter? PlayerCharacter { get; set; }

    public ICollection<PlayerCard> Cards { get; } = new List<PlayerCard>();

    /// <summary>
    /// Gets or creates the player's entry for a card, then unlocks it through a
    /// validated rule. This is the collection-level entry point for unlocking.
    /// </summary>
    public PlayerCard UnlockCard(int cardId, ICardUnlockRule rule, PlayerCharacter playerCharacter)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(playerCharacter);

        if (playerCharacter.Id != PlayerCharacterId)
        {
            throw new InvalidOperationException("The character does not own this card collection.");
        }

        if (!rule.IsValid)
        {
            throw new InvalidOperationException("A card cannot be unlocked with an invalid rule.");
        }

        if (rule.CardId != cardId)
        {
            throw new InvalidOperationException("The unlock rule does not belong to this card.");
        }

        var playerCard = Cards.SingleOrDefault(card => card.CardId == cardId);
        if (playerCard is null)
        {
            playerCard = new PlayerCard(cardId);
            Cards.Add(playerCard);
        }

        playerCard.Unlock(rule, playerCharacter);
        return playerCard;
    }
}
