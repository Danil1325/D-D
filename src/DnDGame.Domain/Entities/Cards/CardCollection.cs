using DnDGame.Domain.Common;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Enums;

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

    /// <summary>Adds a locked card entry whose information follows the supplied visibility rule.</summary>
    public PlayerCard AddLockedCard(int cardId, CardVisibilityRule visibilityRule)
    {
        if (Cards.Any(card => card.CardId == cardId))
        {
            throw new InvalidOperationException("This card already exists in the collection.");
        }

        var playerCard = new PlayerCard(cardId);
        playerCard.SetVisibilityRule(visibilityRule);
        Cards.Add(playerCard);
        return playerCard;
    }

    /// <summary>
    /// Returns a display-safe card projection. Unlocked cards always reveal all
    /// details; locked cards are filtered according to their visibility rule.
    /// </summary>
    public CardDetails GetCardDetails(int cardId, CardVisibilityRules? visibilityRules = null)
    {
        var playerCard = Cards.SingleOrDefault(card => card.CardId == cardId)
            ?? throw new KeyNotFoundException("The card is not in this collection.");
        var card = playerCard.Card
            ?? throw new InvalidOperationException("Card details are unavailable because the card definition is missing.");

        var visibility = playerCard.Unlocked
            ? CardInformationVisibility.Complete
            : (visibilityRules ?? new CardVisibilityRules()).Resolve(playerCard.VisibilityRule);

        return new CardDetails
        {
            CardId = playerCard.CardId,
            Unlocked = playerCard.Unlocked,
            Quantity = playerCard.Unlocked ? playerCard.Quantity : null,
            Name = visibility.ShowName ? card.Name : null,
            Description = visibility.ShowDescription ? card.Description : null,
            CardType = visibility.ShowCardType ? card.CardType : null,
            Category = visibility.ShowCategory ? card.Category : null,
            Rarity = visibility.ShowRarity ? card.Rarity : null,
            BaseCost = visibility.ShowCost ? card.BaseCost : null,
            BaseDamage = visibility.ShowDamage ? card.BaseDamage : null,
            TargetType = visibility.ShowTargetType ? card.TargetType : null,
            EffectType = visibility.ShowEffectType ? card.EffectType : null
        };
    }

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
