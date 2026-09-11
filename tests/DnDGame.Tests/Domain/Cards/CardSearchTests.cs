using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Enums;
using Xunit;

namespace DnDGame.Tests.Domain.Cards;

public class CardSearchTests
{
    [Fact]
    public void SearchCards_FiltersAndSortsWithoutChangingCollectionState()
    {
        var character = new PlayerCharacter { Id = 1, Level = 2 };
        var collection = new CardCollection(character.Id);
        var unlocked = AddUnlocked(collection, character, CreateCard(1, "Arcane Bolt", CardRarity.Rare, CardCategory.Offensive, CardType.Spell, 3));
        var locked = collection.AddLockedCard(2, CardVisibilityRule.ShowNameOnly);
        locked.Card = CreateCard(2, "Bronze Shield", CardRarity.Common, CardCategory.Defensive, CardType.Armor, 1);
        var anotherUnlocked = AddUnlocked(collection, character, CreateCard(3, "Amber Blade", CardRarity.Rare, CardCategory.Offensive, CardType.Weapon, 2));
        var originalCards = collection.Cards.ToList();

        var results = collection.SearchCards("a", new CardSearchOptions
        {
            Rarity = CardRarity.Rare,
            Category = CardCategory.Offensive,
            Unlocked = true,
            SortBy = CardSortBy.EnergyCost,
            SortOrder = SortOrder.Descending
        });

        Assert.Equal(["Arcane Bolt", "Amber Blade"], results.Select(card => card.Name));
        Assert.Equal(originalCards, collection.Cards);
        Assert.True(unlocked.Unlocked);
        Assert.True(anotherUnlocked.Unlocked);
        Assert.False(locked.Unlocked);
        Assert.Equal(1, unlocked.Quantity);
        Assert.Equal(0, locked.Quantity);
    }

    [Fact]
    public void SearchCards_FiltersByTypeAndCanReturnLockedCards()
    {
        var character = new PlayerCharacter { Id = 1, Level = 1 };
        var collection = new CardCollection(character.Id);
        var locked = collection.AddLockedCard(2, CardVisibilityRule.ShowNameOnly);
        locked.Card = CreateCard(2, "Bronze Shield", CardRarity.Common, CardCategory.Defensive, CardType.Armor, 1);
        var unlocked = AddUnlocked(collection, character, CreateCard(3, "Amber Blade", CardRarity.Rare, CardCategory.Offensive, CardType.Weapon, 2));

        var results = collection.SearchCards(null, new CardSearchOptions
        {
            Type = CardType.Armor,
            Unlocked = false,
            SortBy = CardSortBy.Name,
            SortOrder = SortOrder.Ascending
        });

        var result = Assert.Single(results);
        Assert.Equal(locked.CardId, result.CardId);
        Assert.Equal("Bronze Shield", result.Name);
        Assert.False(result.Unlocked);
        Assert.True(unlocked.Unlocked);
    }

    [Fact]
    public void SearchCards_NoFilters_ReturnsAllCardsWithDefinitions()
    {
        var collection = SetupCollection();

        var results = collection.SearchCards(null);

        Assert.Equal(5, results.Count);
        Assert.All(results, card => Assert.NotNull(card.Name));
    }

    [Fact]
    public void SearchCards_FilterByCategory_OnlyMatchesSelected()
    {
        var collection = SetupCollection();

        var results = collection.SearchCards(null, new CardSearchOptions { Category = CardCategory.Healing });

        var result = Assert.Single(results);
        Assert.Equal("Heal Potion", result.Name);
    }

    [Fact]
    public void SearchCards_FilterByRarity_OnlyMatchesSelected()
    {
        var collection = SetupCollection();

        var results = collection.SearchCards(null, new CardSearchOptions { Rarity = CardRarity.Legendary });

        var result = Assert.Single(results);
        Assert.Equal("Dragon Blade", result.Name);
    }

    [Fact]
    public void SearchCards_FilterByUnlockedFalse_ReturnsOnlyLocked()
    {
        var collection = SetupCollection();

        var results = collection.SearchCards(null, new CardSearchOptions { Unlocked = false });

        var result = Assert.Single(results);
        Assert.Equal("Mysterious Scroll", result.Name);
        Assert.False(result.Unlocked);
    }

    [Fact]
    public void SearchCards_FilterByUnlockedTrue_ReturnsOnlyUnlocked()
    {
        var collection = SetupCollection();

        var results = collection.SearchCards(null, new CardSearchOptions { Unlocked = true });

        Assert.Equal(4, results.Count);
        Assert.All(results, card => Assert.True(card.Unlocked));
    }

    [Fact]
    public void SearchCards_SortByRarityDescending()
    {
        var collection = SetupCollection();

        var results = collection.SearchCards(null, new CardSearchOptions
        {
            SortBy = CardSortBy.Rarity,
            SortOrder = SortOrder.Descending
        });

        // Legendary (Dragon Blade) > Rare ties by name (Arcane Bolt, Iron Shield, Mysterious Scroll) > Uncommon (Heal Potion)
        Assert.Equal(
            new[] { "Dragon Blade", "Arcane Bolt", "Iron Shield", "Mysterious Scroll", "Heal Potion" },
            results.Select(r => r.Name));
    }

    [Fact]
    public void SearchCards_SortByEnergyCostAscending()
    {
        var collection = SetupCollection();

        var results = collection.SearchCards(null, new CardSearchOptions
        {
            SortBy = CardSortBy.EnergyCost,
            SortOrder = SortOrder.Ascending
        });

        // 1 (Heal Potion), 2 (Iron Shield), 3 (Arcane Bolt), then cost-5 ties by name (Dragon Blade, Mysterious Scroll)
        Assert.Equal(
            new[] { "Heal Potion", "Iron Shield", "Arcane Bolt", "Dragon Blade", "Mysterious Scroll" },
            results.Select(r => r.Name));
    }

    [Fact]
    public void SearchCards_SortByNameDescending()
    {
        var collection = SetupCollection();

        var results = collection.SearchCards(null, new CardSearchOptions
        {
            SortBy = CardSortBy.Name,
            SortOrder = SortOrder.Descending
        });

        var names = results.Select(r => r.Name!).ToList();
        Assert.Equal(
            new[] { "Mysterious Scroll", "Iron Shield", "Heal Potion", "Dragon Blade", "Arcane Bolt" },
            names);
    }

    [Fact]
    public void SearchCards_CaseInsensitiveNameSearch()
    {
        var collection = SetupCollection();

        var upper = collection.SearchCards("ARCANE BOLT");
        var lower = collection.SearchCards("arcane bolt");
        var mixed = collection.SearchCards("ArCaNe BoLt");

        Assert.Single(upper);
        Assert.Single(lower);
        Assert.Single(mixed);
        Assert.Equal("Arcane Bolt", upper[0].Name);
    }

    [Fact]
    public void SearchCards_SearchAgainstDescription()
    {
        var collection = SetupCollection();

        var results = collection.SearchCards("block");

        var result = Assert.Single(results);
        Assert.Equal("Iron Shield", result.Name);
    }

    [Fact]
    public void SearchCards_EmptyResultForNoMatch()
    {
        var collection = SetupCollection();

        var results = collection.SearchCards("xyznonexistent");

        Assert.Empty(results);
    }

    [Fact]
    public void SearchCards_MultipleFiltersCombineCorrectly()
    {
        var collection = SetupCollection();

        var results = collection.SearchCards(null, new CardSearchOptions
        {
            Rarity = CardRarity.Rare,
            Type = CardType.Spell,
            Unlocked = true
        });

        var unlocked = Assert.Single(results);
        Assert.Equal("Arcane Bolt", unlocked.Name);

        var lockedCount = collection.SearchCards(null, new CardSearchOptions
        {
            Rarity = CardRarity.Rare,
            Type = CardType.Spell
        }).Count;
        Assert.Equal(2, lockedCount);
    }

    [Fact]
    public void SearchCards_LockedCardRespectsVisibilityInResults()
    {
        var collection = SetupCollection();

        var results = collection.SearchCards(null, new CardSearchOptions { Unlocked = false });

        var locked = Assert.Single(results);
        Assert.Equal("Mysterious Scroll", locked.Name);
        Assert.Null(locked.Description);
        Assert.Null(locked.BaseCost);
        Assert.Null(locked.BaseDamage);
    }

    [Fact]
    public void SearchCards_PreservesOwnershipQuantityAndVisibilityState()
    {
        var character = new PlayerCharacter { Id = 1, Level = 3 };
        var collection = new CardCollection(character.Id);

        var unlocked = collection.UnlockCard(1, new LevelCardUnlockRule(1, 1), character);
        unlocked.Card = CreateCard(1, "Card A", CardRarity.Common, CardCategory.Offensive, CardType.Weapon, 2);
        unlocked.AddCopies(2);

        var unlocked2 = collection.UnlockCard(2, new LevelCardUnlockRule(2, 1), character);
        unlocked2.Card = CreateCard(2, "Card B", CardRarity.Rare, CardCategory.Healing, CardType.Spell, 3);

        var lockedCard = collection.AddLockedCard(3, CardVisibilityRule.ShowNameOnly);
        lockedCard.Card = CreateCard(3, "Card C", CardRarity.Legendary, CardCategory.Defensive, CardType.Armor, 1);

        var snap1 = (unlocked.Unlocked, unlocked.Quantity, unlocked.VisibilityRule);
        var snap2 = (unlocked2.Unlocked, unlocked2.Quantity, unlocked2.VisibilityRule);
        var snap3 = (lockedCard.Unlocked, lockedCard.Quantity, lockedCard.VisibilityRule);
        var cardCount = collection.Cards.Count;

        collection.SearchCards("Card");
        collection.SearchCards(null, new CardSearchOptions { Rarity = CardRarity.Rare });
        collection.SearchCards(null, new CardSearchOptions { Category = CardCategory.Healing });
        collection.SearchCards(null, new CardSearchOptions { Type = CardType.Weapon, Unlocked = true });
        collection.SearchCards("nonexistent");
        collection.SearchCards(null, new CardSearchOptions { SortBy = CardSortBy.Rarity, SortOrder = SortOrder.Descending });
        collection.SearchCards(null, new CardSearchOptions { SortBy = CardSortBy.Name, SortOrder = SortOrder.Ascending });
        collection.SearchCards("Card A", new CardSearchOptions { SortBy = CardSortBy.EnergyCost, SortOrder = SortOrder.Ascending });

        Assert.Equal(cardCount, collection.Cards.Count);
        Assert.Same(unlocked, collection.Cards.Single(c => c.CardId == 1));
        Assert.Same(unlocked2, collection.Cards.Single(c => c.CardId == 2));
        Assert.Same(lockedCard, collection.Cards.Single(c => c.CardId == 3));
        Assert.Equal(snap1, (unlocked.Unlocked, unlocked.Quantity, unlocked.VisibilityRule));
        Assert.Equal(snap2, (unlocked2.Unlocked, unlocked2.Quantity, unlocked2.VisibilityRule));
        Assert.Equal(snap3, (lockedCard.Unlocked, lockedCard.Quantity, lockedCard.VisibilityRule));
    }

    [Fact]
    public void SearchCards_SortByRarityAscending()
    {
        var collection = SetupCollection();

        var results = collection.SearchCards(null, new CardSearchOptions
        {
            SortBy = CardSortBy.Rarity,
            SortOrder = SortOrder.Ascending
        });

        // Uncommon (Heal Potion) < Rare ties by name (Arcane Bolt, Iron Shield, Mysterious Scroll) < Legendary (Dragon Blade)
        Assert.Equal(
            new[] { "Heal Potion", "Arcane Bolt", "Iron Shield", "Mysterious Scroll", "Dragon Blade" },
            results.Select(r => r.Name));
    }

    [Fact]
    public void SearchCards_TrimmedSearchTerm()
    {
        var collection = SetupCollection();

        var trimmed = collection.SearchCards("  Arcane  ");
        var untrimmed = collection.SearchCards("Arcane");

        Assert.Equal(
            trimmed.Select(r => r.CardId),
            untrimmed.Select(r => r.CardId));
    }

    private static CardCollection SetupCollection()
    {
        var character = new PlayerCharacter { Id = 1, Level = 3 };
        var collection = new CardCollection(character.Id);

        var fireball = collection.UnlockCard(1, new LevelCardUnlockRule(1, 1), character);
        fireball.Card = CreateCard(1, "Arcane Bolt", CardRarity.Rare, CardCategory.Offensive, CardType.Spell, 3, "Fires a magical bolt");

        var heal = collection.UnlockCard(2, new LevelCardUnlockRule(2, 1), character);
        heal.Card = CreateCard(2, "Heal Potion", CardRarity.Uncommon, CardCategory.Healing, CardType.Item, 1, "Restores health to the drinker");

        var shield = collection.UnlockCard(3, new LevelCardUnlockRule(3, 1), character);
        shield.Card = CreateCard(3, "Iron Shield", CardRarity.Rare, CardCategory.Defensive, CardType.Armor, 2, "Blocks incoming attacks");

        var sword = collection.UnlockCard(4, new LevelCardUnlockRule(4, 1), character);
        sword.Card = CreateCard(4, "Dragon Blade", CardRarity.Legendary, CardCategory.Offensive, CardType.Weapon, 5, "A legendary sword");

        var locked = collection.AddLockedCard(5, CardVisibilityRule.ShowNameOnly);
        locked.Card = CreateCard(5, "Mysterious Scroll", CardRarity.Rare, CardCategory.Utility, CardType.Spell, 5, "Its power is unknown");

        return collection;
    }

    private static PlayerCard AddUnlocked(CardCollection collection, PlayerCharacter character, Card card)
    {
        var playerCard = collection.UnlockCard(card.Id, new LevelCardUnlockRule(card.Id, 1), character);
        playerCard.Card = card;
        return playerCard;
    }

    private static Card CreateCard(
        int id, string name, CardRarity rarity, CardCategory category,
        CardType type, int energyCost, string? description = null) => new()
    {
        Id = id,
        Name = name,
        Description = description ?? name,
        Rarity = rarity,
        Category = category,
        CardType = type,
        BaseCost = energyCost
    };
}
