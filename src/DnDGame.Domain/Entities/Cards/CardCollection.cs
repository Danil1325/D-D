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

        return CreateCardDetails(playerCard, visibilityRules ?? new CardVisibilityRules());
    }

    /// <summary>
    /// Searches cards without changing collection membership, ownership, quantity,
    /// visibility, or unlock state. Filters use the stored card definition while
    /// returned data continues to honor locked-card visibility rules.
    /// </summary>
    public IReadOnlyList<CardDetails> SearchCards(
        string? query,
        CardSearchOptions? options = null,
        CardVisibilityRules? visibilityRules = null)
    {
        options ??= new CardSearchOptions();
        var rules = visibilityRules ?? new CardVisibilityRules();
        var searchTerm = query?.Trim();

        IEnumerable<PlayerCard> matchingCards = Cards.Where(playerCard => playerCard.Card is not null);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            matchingCards = matchingCards.Where(playerCard =>
                playerCard.Card!.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                playerCard.Card.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (options.Rarity is not null)
        {
            matchingCards = matchingCards.Where(playerCard => playerCard.Card!.Rarity == options.Rarity);
        }

        if (options.Category is not null)
        {
            matchingCards = matchingCards.Where(playerCard => playerCard.Card!.Category == options.Category);
        }

        if (options.Type is not null)
        {
            matchingCards = matchingCards.Where(playerCard => playerCard.Card!.CardType == options.Type);
        }

        if (options.Unlocked is not null)
        {
            matchingCards = matchingCards.Where(playerCard => playerCard.Unlocked == options.Unlocked);
        }

        return Sort(matchingCards, options)
            .Select(playerCard => CreateCardDetails(playerCard, rules))
            .ToList();
    }

    private static IOrderedEnumerable<PlayerCard> Sort(
        IEnumerable<PlayerCard> cards,
        CardSearchOptions options)
    {
        return (options.SortBy, options.SortOrder) switch
        {
            (CardSortBy.Name, SortOrder.Ascending) => cards.OrderBy(card => card.Card!.Name, StringComparer.OrdinalIgnoreCase).ThenBy(card => card.CardId),
            (CardSortBy.Name, SortOrder.Descending) => cards.OrderByDescending(card => card.Card!.Name, StringComparer.OrdinalIgnoreCase).ThenBy(card => card.CardId),
            (CardSortBy.Rarity, SortOrder.Ascending) => cards.OrderBy(card => card.Card!.Rarity).ThenBy(card => card.Card!.Name, StringComparer.OrdinalIgnoreCase),
            (CardSortBy.Rarity, SortOrder.Descending) => cards.OrderByDescending(card => card.Card!.Rarity).ThenBy(card => card.Card!.Name, StringComparer.OrdinalIgnoreCase),
            (CardSortBy.EnergyCost, SortOrder.Ascending) => cards.OrderBy(card => card.Card!.BaseCost).ThenBy(card => card.Card!.Name, StringComparer.OrdinalIgnoreCase),
            (CardSortBy.EnergyCost, SortOrder.Descending) => cards.OrderByDescending(card => card.Card!.BaseCost).ThenBy(card => card.Card!.Name, StringComparer.OrdinalIgnoreCase),
            _ => throw new ArgumentOutOfRangeException(nameof(options), "Unknown card sorting options.")
        };
    }

    private static CardDetails CreateCardDetails(PlayerCard playerCard, CardVisibilityRules visibilityRules)
    {
        var card = playerCard.Card
            ?? throw new InvalidOperationException("Card details are unavailable because the card definition is missing.");

        var visibility = playerCard.Unlocked
            ? CardInformationVisibility.Complete
            : visibilityRules.Resolve(playerCard.VisibilityRule);

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
