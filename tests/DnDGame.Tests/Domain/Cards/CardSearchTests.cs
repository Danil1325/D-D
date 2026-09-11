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

    private static PlayerCard AddUnlocked(CardCollection collection, PlayerCharacter character, Card card)
    {
        var playerCard = collection.UnlockCard(card.Id, new LevelCardUnlockRule(card.Id, 1), character);
        playerCard.Card = card;
        return playerCard;
    }

    private static Card CreateCard(int id, string name, CardRarity rarity, CardCategory category, CardType type, int energyCost) => new()
    {
        Id = id,
        Name = name,
        Description = name,
        Rarity = rarity,
        Category = category,
        CardType = type,
        BaseCost = energyCost
    };
}
