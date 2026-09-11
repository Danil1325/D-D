using DnDGame.Domain.Common;
using DnDGame.Domain.Entities.Characters;

namespace DnDGame.Domain.Entities.Cards;

/// <summary>A card owned by a player, including its availability and copy count.</summary>
public class PlayerCard : BaseEntity
{
    public PlayerCard(int cardId)
    {
        if (cardId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cardId), "Card ID must be positive.");
        }

        CardId = cardId;
    }

    public int CardId { get; }

    public Card? Card { get; set; }

    public bool Unlocked { get; private set; }

    public int Quantity { get; private set; }

    /// <summary>
    /// Unlocks this card only when its matching rule is structurally valid and
    /// the owning character meets the rule's condition.
    /// </summary>
    public void Unlock(ICardUnlockRule rule, PlayerCharacter playerCharacter)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(playerCharacter);

        if (!rule.IsValid)
        {
            throw new InvalidOperationException("A card cannot be unlocked with an invalid rule.");
        }

        if (rule.CardId != CardId)
        {
            throw new InvalidOperationException("The unlock rule does not belong to this card.");
        }

        if (!rule.IsSatisfiedBy(playerCharacter))
        {
            throw new InvalidOperationException("The unlock rule is not satisfied by this character.");
        }

        Unlocked = true;
        Quantity = Math.Max(Quantity, 1);
    }

    public void AddCopies(int quantity)
    {
        if (!Unlocked)
        {
            throw new InvalidOperationException("A locked card cannot receive copies.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        Quantity += quantity;
    }
}
