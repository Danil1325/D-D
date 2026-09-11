using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Characters;
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
}
