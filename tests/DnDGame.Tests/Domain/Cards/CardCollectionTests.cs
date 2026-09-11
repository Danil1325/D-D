using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Enums;
using Xunit;

namespace DnDGame.Tests.Domain.Cards;

public class CardCollectionTests
{
    [Fact]
    public void UnlockCard_WithValidSatisfiedRule_UnlocksAndAddsOneCopy()
    {
        var character = new PlayerCharacter { Id = 7, Level = 3 };
        var collection = new CardCollection(character.Id);

        var playerCard = collection.UnlockCard(12, new LevelCardUnlockRule(12, requiredLevel: 3), character);

        Assert.True(playerCard.Unlocked);
        Assert.Equal(1, playerCard.Quantity);
        Assert.Equal(12, playerCard.CardId);
        Assert.Single(collection.Cards);
    }

    [Fact]
    public void UnlockCard_WithInvalidRule_ThrowsAndDoesNotUnlockCard()
    {
        var character = new PlayerCharacter { Id = 7, Level = 3 };
        var collection = new CardCollection(character.Id);

        Assert.Throws<InvalidOperationException>(
            () => collection.UnlockCard(12, new LevelCardUnlockRule(12, requiredLevel: 0), character));
        Assert.Empty(collection.Cards);
    }

    [Fact]
    public void UnlockCard_WithUnsatisfiedRule_ThrowsAndKeepsCardLocked()
    {
        var character = new PlayerCharacter { Id = 7, Level = 2 };
        var collection = new CardCollection(character.Id);

        Assert.Throws<InvalidOperationException>(
            () => collection.UnlockCard(12, new LevelCardUnlockRule(12, requiredLevel: 3), character));

        var playerCard = Assert.Single(collection.Cards);
        Assert.False(playerCard.Unlocked);
        Assert.Equal(0, playerCard.Quantity);
    }

    [Fact]
    public void Constructor_ThrowsForNonPositivePlayerCharacterId()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CardCollection(0));
    }

    [Fact]
    public void UnlockCard_ThrowsWhenCharacterIsNotCollectionOwner()
    {
        var collection = new CardCollection(playerCharacterId: 1);
        var otherCharacter = new PlayerCharacter { Id = 99, Level = 5 };

        Assert.Throws<InvalidOperationException>(
            () => collection.UnlockCard(5, new LevelCardUnlockRule(5, requiredLevel: 3), otherCharacter));
        Assert.Empty(collection.Cards);
    }

    [Fact]
    public void UnlockCard_ThrowsWhenRuleCardIdMismatch()
    {
        var collection = new CardCollection(playerCharacterId: 1);
        var character = new PlayerCharacter { Id = 1, Level = 5 };

        Assert.Throws<InvalidOperationException>(
            () => collection.UnlockCard(cardId: 99, new LevelCardUnlockRule(cardId: 10, requiredLevel: 3), character));
    }

    [Fact]
    public void UnlockCard_CardAlreadyExists_KeepsExistingAndUnlocks()
    {
        var collection = new CardCollection(playerCharacterId: 1);
        var character = new PlayerCharacter { Id = 1, Level = 5 };
        var existing = collection.AddLockedCard(5, CardVisibilityRule.ShowNameOnly);

        var returned = collection.UnlockCard(5, new LevelCardUnlockRule(5, requiredLevel: 3), character);

        Assert.Same(existing, returned);
        Assert.True(returned.Unlocked);
        Assert.Equal(1, returned.Quantity);
        Assert.Single(collection.Cards);
    }

    [Fact]
    public void UnlockCard_RepeatedUnlockDoesNotIncrementQuantity()
    {
        var collection = new CardCollection(playerCharacterId: 1);
        var character = new PlayerCharacter { Id = 1, Level = 5 };
        var rule = new LevelCardUnlockRule(5, requiredLevel: 3);

        var first = collection.UnlockCard(5, rule, character);
        first.AddCopies(2);

        var second = collection.UnlockCard(5, rule, character);

        Assert.Same(first, second);
        Assert.Equal(3, second.Quantity);
    }

    [Fact]
    public void AddLockedCard_ThrowsForDuplicateCardId()
    {
        var collection = new CardCollection(playerCharacterId: 1);
        collection.AddLockedCard(1, CardVisibilityRule.HideAll);

        var duplicate = Assert.Throws<InvalidOperationException>(
            () => collection.AddLockedCard(1, CardVisibilityRule.ShowNameOnly));
        Assert.Single(collection.Cards);
    }

    [Fact]
    public void AddLockedCard_ThrowsForNonPositiveCardId()
    {
        var collection = new CardCollection(playerCharacterId: 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => collection.AddLockedCard(0, CardVisibilityRule.HideAll));
    }

    [Fact]
    public void GetCardDetails_ThrowsForUnknownCard()
    {
        var collection = new CardCollection(playerCharacterId: 1);

        Assert.Throws<KeyNotFoundException>(() => collection.GetCardDetails(999));
    }

    [Fact]
    public void UnlockCard_MultipleCardsCoexistInCollection()
    {
        var collection = new CardCollection(playerCharacterId: 1);
        var character = new PlayerCharacter { Id = 1, Level = 5 };

        var first = collection.UnlockCard(10, new LevelCardUnlockRule(10, requiredLevel: 1), character);
        first.Card = new Card { Id = 10, Name = "Alpha" };
        var second = collection.UnlockCard(20, new LevelCardUnlockRule(20, requiredLevel: 1), character);
        second.Card = new Card { Id = 20, Name = "Beta" };

        Assert.Equal(2, collection.Cards.Count);
        Assert.All(collection.Cards, card => Assert.True(card.Unlocked));
    }

    [Fact]
    public void PlayerCard_AddCopies_ThrowsForLockedCard()
    {
        var collection = new CardCollection(playerCharacterId: 1);
        var locked = collection.AddLockedCard(1, CardVisibilityRule.HideAll);

        Assert.Throws<InvalidOperationException>(() => locked.AddCopies(3));
        Assert.Equal(0, locked.Quantity);
    }

    [Fact]
    public void PlayerCard_AddCopies_ThrowsForZeroQuantity()
    {
        var collection = new CardCollection(playerCharacterId: 1);
        var character = new PlayerCharacter { Id = 1, Level = 5 };
        var unlocked = collection.UnlockCard(1, new LevelCardUnlockRule(1, requiredLevel: 1), character);

        Assert.Throws<ArgumentOutOfRangeException>(() => unlocked.AddCopies(0));
    }
}
