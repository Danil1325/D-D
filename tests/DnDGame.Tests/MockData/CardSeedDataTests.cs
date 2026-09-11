using DnDGame.Domain.Enums;
using DnDGame.MockData;
using Xunit;

namespace DnDGame.Tests.MockData;

public class CardSeedDataTests
{
    [Fact]
    public void SeededStore_ContainsTheCompleteRequestedCardCatalogue()
    {
        var store = MockDataBootstrapper.CreateSeededStore();

        Assert.Equal(60, store.Cards.Count);
        Assert.Equal(60, store.Cards.Select(card => card.Id).Distinct().Count());
    }

    [Fact]
    public void SeededCards_PreserveSpecifiedCostsEffectsAndModifiers()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var staff = Assert.Single(store.Cards, card => card.Name == "The Arcane Staff");
        var bow = Assert.Single(store.Cards, card => card.Name == "The Elven Bow");
        var potion = Assert.Single(store.Cards, card => card.Name == "Potion of Superior Healing");

        Assert.Equal(18, staff.ResourceCost);
        Assert.Equal("Arcane Surge", staff.ActiveEffect);
        Assert.Equal("Restores 10 Mana after a Critical Hit", staff.PassiveEffect);
        Assert.Equal("+6 Intelligence", staff.Bonus);
        Assert.Equal("-12% Physical Defense", staff.Penalty);

        Assert.Equal(24, bow.BaseDamage);
        Assert.Equal("Piercing", bow.DamageType);
        Assert.Equal("+30% Critical Chance at Long Range", bow.PassiveEffect);

        Assert.Equal(CardType.Consumable, potion.CardType);
        Assert.Equal(CardCategory.Healing, potion.Category);
        Assert.Equal(35, potion.Power);
        Assert.Equal("Superior Restoration", potion.ActiveEffect);
        Assert.Equal("Removes Bleeding, Poison and Burn", potion.Bonus);
        Assert.Equal(EffectType.Healing, potion.EffectType);
    }
}
