using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Enums;
using Xunit;

namespace DnDGame.Tests.Domain.Cards;

public class CardVisibilityTests
{
    [Theory]
    [InlineData(CardVisibilityRule.HideAll, null, null, null)]
    [InlineData(CardVisibilityRule.ShowNameOnly, "Fireball", null, null)]
    [InlineData(CardVisibilityRule.HideStatistics, "Fireball", "Damage spell", null)]
    [InlineData(CardVisibilityRule.ShowBasicInformation, "Fireball", "Damage spell", null)]
    public void LockedCard_UsesItsVisibilityRule(
        CardVisibilityRule rule,
        string? expectedName,
        string? expectedDescription,
        int? expectedDamage)
    {
        var collection = new CardCollection(1);
        var playerCard = collection.AddLockedCard(9, rule);
        playerCard.Card = CreateCard();

        var details = collection.GetCardDetails(9);

        Assert.False(details.Unlocked);
        Assert.Equal(expectedName, details.Name);
        Assert.Equal(expectedDescription, details.Description);
        Assert.Equal(expectedDamage, details.BaseDamage);
    }

    [Fact]
    public void LockedCard_UsesCustomizableVisibilityPreset()
    {
        var collection = new CardCollection(1);
        var playerCard = collection.AddLockedCard(9, CardVisibilityRule.ShowNameOnly);
        playerCard.Card = CreateCard();
        var rules = new CardVisibilityRules
        {
            ShowNameOnly = new CardInformationVisibility(ShowDescription: true)
        };

        var details = collection.GetCardDetails(9, rules);

        Assert.Null(details.Name);
        Assert.Equal("Damage spell", details.Description);
    }

    [Fact]
    public void UnlockedCard_AlwaysReturnsCompleteInformation()
    {
        var character = new PlayerCharacter { Id = 1, Level = 1 };
        var collection = new CardCollection(character.Id);
        var playerCard = collection.UnlockCard(9, new LevelCardUnlockRule(9, 1), character);
        playerCard.Card = CreateCard();
        playerCard.SetVisibilityRule(CardVisibilityRule.HideAll);

        var details = collection.GetCardDetails(9);

        Assert.True(details.Unlocked);
        Assert.Equal("Fireball", details.Name);
        Assert.Equal("Damage spell", details.Description);
        Assert.Equal(10, details.BaseDamage);
        Assert.Equal(2, details.BaseCost);
        Assert.Equal(1, details.Quantity);
    }

    [Fact]
    public void LockedCard_HideStatistics_ShowsNameDescriptionTypeCategoryRarityAndTargetButNotCostOrDamage()
    {
        var collection = new CardCollection(1);
        var playerCard = collection.AddLockedCard(9, CardVisibilityRule.HideStatistics);
        playerCard.Card = CreateCard();

        var details = collection.GetCardDetails(9);

        Assert.False(details.Unlocked);
        Assert.Equal("Fireball", details.Name);
        Assert.Equal("Damage spell", details.Description);
        Assert.Equal(CardType.Spell, details.CardType);
        Assert.Equal(CardCategory.Offensive, details.Category);
        Assert.Equal(CardRarity.Rare, details.Rarity);
        Assert.Equal(TargetType.SingleEnemy, details.TargetType);
        Assert.Equal(EffectType.Damage, details.EffectType);
        Assert.Null(details.BaseCost);
        Assert.Null(details.BaseDamage);
        Assert.Null(details.Quantity);
    }

    [Fact]
    public void LockedCard_ShowBasicInformation_OmitsCostDamageTargetAndEffect()
    {
        var collection = new CardCollection(1);
        var playerCard = collection.AddLockedCard(9, CardVisibilityRule.ShowBasicInformation);
        playerCard.Card = CreateCard();

        var details = collection.GetCardDetails(9);

        Assert.False(details.Unlocked);
        Assert.Equal("Fireball", details.Name);
        Assert.Equal("Damage spell", details.Description);
        Assert.Equal(CardType.Spell, details.CardType);
        Assert.Equal(CardCategory.Offensive, details.Category);
        Assert.Equal(CardRarity.Rare, details.Rarity);
        Assert.Null(details.BaseCost);
        Assert.Null(details.BaseDamage);
        Assert.Null(details.TargetType);
        Assert.Null(details.EffectType);
        Assert.Null(details.Quantity);
    }

    [Fact]
    public void GetCardDetails_ForCardNotInCollection_Throws()
    {
        var collection = new CardCollection(1);

        Assert.Throws<KeyNotFoundException>(() => collection.GetCardDetails(42));
    }

    [Fact]
    public void UnlockedCardWithMultipleCopies_ReturnsQuantity()
    {
        var character = new PlayerCharacter { Id = 1, Level = 1 };
        var collection = new CardCollection(character.Id);
        var playerCard = collection.UnlockCard(9, new LevelCardUnlockRule(9, 1), character);
        playerCard.Card = CreateCard();
        playerCard.AddCopies(3);

        var details = collection.GetCardDetails(9);

        Assert.True(details.Unlocked);
        Assert.Equal(4, details.Quantity);
        Assert.Equal("Fireball", details.Name);
        Assert.Equal(10, details.BaseDamage);
    }

    [Fact]
    public void LockedCard_DefaultVisibilityIsHideAll()
    {
        var collection = new CardCollection(1);
        var playerCard = collection.AddLockedCard(9, CardVisibilityRule.HideAll);
        playerCard.Card = CreateCard();

        Assert.Equal(CardVisibilityRule.HideAll, playerCard.VisibilityRule);
    }

    [Fact]
    public void SetVisibilityRule_ChangesLockedCardAppearance()
    {
        var collection = new CardCollection(1);
        var playerCard = collection.AddLockedCard(9, CardVisibilityRule.HideAll);
        playerCard.Card = CreateCard();

        var before = collection.GetCardDetails(9);
        Assert.Null(before.Name);

        playerCard.SetVisibilityRule(CardVisibilityRule.ShowNameOnly);

        var after = collection.GetCardDetails(9);
        Assert.Equal("Fireball", after.Name);
        Assert.Null(after.Description);
    }

    private static Card CreateCard() => new()
    {
        Id = 9,
        Name = "Fireball",
        Description = "Damage spell",
        CardType = CardType.Spell,
        Category = CardCategory.Offensive,
        Rarity = CardRarity.Rare,
        BaseCost = 2,
        BaseDamage = 10,
        TargetType = TargetType.SingleEnemy,
        EffectType = EffectType.Damage
    };
}
