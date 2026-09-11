using DnDGame.BusinessLayer.Engines;
using DnDGame.BusinessLayer.Repositories;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;
using Xunit;

namespace DnDGame.Tests.BusinessLayer;

public class DeckAndHandEngineTests
{
    [Fact]
    public void ValidateDeck_ReturnsEngineResultForSizeAndCopyViolations()
    {
        var validator = new DeckValidator(new DeckRules(minimumDeckSize: 2, maximumDeckSize: 3, maximumCopiesPerCard: 1));
        var tooSmall = new Deck { Cards = [Card(1)] };
        var duplicate = new Deck { Cards = [Card(1), Card(1)] };

        var smallResult = validator.ValidateDeck(tooSmall);
        var duplicateResult = validator.ValidateDeck(duplicate);

        Assert.False(smallResult.IsSuccess);
        Assert.Equal(ErrorCode.DECK_TOO_SMALL, smallResult.ErrorCode);
        Assert.False(duplicateResult.IsSuccess);
        Assert.Equal(ErrorCode.CARD_COPY_LIMIT_REACHED, duplicateResult.ErrorCode);
    }

    [Fact]
    public void ShuffleDeck_PreservesAllCards()
    {
        var engine = new DeckEngine();
        IList<CardInstance> cards = [Instance(1), Instance(2), Instance(3), Instance(4)];

        engine.ShuffleDeck(cards);

        Assert.Equal([1, 2, 3, 4], cards.Select(card => card.CardId).Order());
    }

    [Fact]
    public void DrawDiscardAndReshuffle_MoveCardsBetweenPiles()
    {
        var engine = new DeckEngine();
        var first = Instance(1);
        var recycled = Instance(2);
        var deck = BattleDeck(draw: [first], discard: [recycled]);

        var drawn = engine.DrawCard(deck);
        var discarded = engine.DiscardCard(deck, drawn!);
        var redrawn = engine.DrawCard(deck);

        Assert.Same(first, drawn);
        Assert.True(discarded);
        Assert.Contains(redrawn, new[] { first, recycled });
        Assert.Empty(deck.DiscardPile);
        Assert.Single(deck.DrawPile);
        Assert.Single(deck.Hand);
        Assert.Equal(2, deck.TotalCardsDrawn);
        Assert.Equal(1, deck.TotalCardsDiscarded);
    }

    [Fact]
    public void HandLimitAndAbsentCard_ReturnEngineResultErrors()
    {
        var handEngine = new HandEngine(new HandRules(maxHandSize: 1));
        var held = Instance(1);
        var missing = Instance(2);
        var deck = BattleDeck(hand: [held]);

        var fullResult = handEngine.AddCard(deck, missing);
        var missingResult = handEngine.RemoveCard(deck, missing);

        Assert.False(fullResult.IsSuccess);
        Assert.Equal(ErrorCode.HAND_FULL, fullResult.ErrorCode);
        Assert.False(missingResult.IsSuccess);
        Assert.Equal(ErrorCode.CARD_NOT_IN_HAND, missingResult.ErrorCode);
    }

    [Fact]
    public void DiscardHand_ReturnsSuccessAndMovesEveryCard()
    {
        var handEngine = new HandEngine(new HandRules(5));
        var deck = BattleDeck(hand: [Instance(1), Instance(2)]);

        var result = handEngine.DiscardHand(deck);

        Assert.True(result.IsSuccess);
        Assert.Empty(deck.Hand);
        Assert.Equal(2, deck.DiscardPile.Count);
        Assert.Equal(2, deck.TotalCardsDiscarded);
    }

    [Fact]
    public void ValidateDeck_ReturnsSuccessForDeckExactlyAtLimits()
    {
        var validator = new DeckValidator(new DeckRules(minimumDeckSize: 2, maximumDeckSize: 3, maximumCopiesPerCard: 1));
        var exactDeck = new Deck { Cards = [Card(1), Card(2)] };

        var result = validator.ValidateDeck(exactDeck);

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorCode);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateDeck_ReturnsDeckTooLargeForDeckAboveMaximum()
    {
        var validator = new DeckValidator(new DeckRules(minimumDeckSize: 2, maximumDeckSize: 2, maximumCopiesPerCard: 1));

        var result = validator.ValidateDeck(new Deck { Cards = [Card(1), Card(2), Card(3)] });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.DECK_TOO_LARGE, result.ErrorCode);
    }

    [Fact]
    public void ValidateDeck_ReturnsDeckInvalidForNullDeck()
    {
        var validator = new DeckValidator(new DeckRules(2, 3, 1));

        var result = validator.ValidateDeck(null!);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.DECK_INVALID, result.ErrorCode);
    }

    [Fact]
    public void ValidateDeck_ReturnsDeckInvalidForNullCardsCollection()
    {
        var validator = new DeckValidator(new DeckRules(2, 3, 1));
        var deck = new Deck { Cards = null! };

        var result = validator.ValidateDeck(deck);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.DECK_INVALID, result.ErrorCode);
    }

    [Fact]
    public void ValidateDeck_ReturnsDeckInvalidForNullCardInDeck()
    {
        var validator = new DeckValidator(new DeckRules(2, 3, 1));

        var result = validator.ValidateDeck(new Deck { Cards = [Card(1), null!] });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.DECK_INVALID, result.ErrorCode);
    }

    [Fact]
    public void ShuffleDeck_OnSingleCard_LeavesCardUnchanged()
    {
        var engine = new DeckEngine();
        var single = Instance(1);
        IList<CardInstance> cards = [single];

        engine.ShuffleDeck(cards);

        Assert.Single(cards);
        Assert.Same(single, cards[0]);
    }

    [Fact]
    public void CreateBattleDeck_ConvertsEveryCardToAnInstanceInTheDrawPile()
    {
        var engine = new DeckEngine();
        var deck = new Deck { Id = 7, Cards = [Card(1), Card(2), Card(3)] };

        var battleDeck = engine.CreateBattleDeck(deck);

        Assert.Equal(7, battleDeck.DeckId);
        Assert.Equal(3, battleDeck.DrawPile.Count);
        Assert.Empty(battleDeck.DiscardPile);
        Assert.Empty(battleDeck.Hand);
        Assert.Equal(0, battleDeck.TotalCardsDrawn);
        Assert.Equal(0, battleDeck.TotalCardsDiscarded);
        Assert.Equal([1, 2, 3], battleDeck.DrawPile.Select(card => card.CardId).Order());
    }

    [Fact]
    public void DrawCard_ReturnsNullWhenBothPilesAreEmpty()
    {
        var engine = new DeckEngine();
        var deck = BattleDeck();

        var drawn = engine.DrawCard(deck);

        Assert.Null(drawn);
        Assert.Empty(deck.Hand);
    }

    [Fact]
    public void DrawCards_StopsWhenTheDeckIsExhausted()
    {
        var engine = new DeckEngine();
        var deck = BattleDeck(draw: [Instance(1), Instance(2)]);

        var drawn = engine.DrawCards(deck, count: 5);

        Assert.Equal(2, drawn.Count);
        Assert.Equal(2, deck.Hand.Count);
        Assert.Empty(deck.DrawPile);
    }

    [Fact]
    public void DrawCards_ThrowsForNonPositiveCount()
    {
        var engine = new DeckEngine();
        var deck = BattleDeck(draw: [Instance(1)]);

        Assert.Throws<ArgumentException>(() => engine.DrawCards(deck, 0));
    }

    [Fact]
    public void DiscardCard_ReturnsFalseForCardNotInHand()
    {
        var engine = new DeckEngine();
        var deck = BattleDeck(hand: [Instance(1)]);

        var result = engine.DiscardCard(deck, Instance(9));

        Assert.False(result);
        Assert.Empty(deck.DiscardPile);
        Assert.Single(deck.Hand);
    }

    [Fact]
    public void ReshuffleDiscardPile_MovesAllDiscardedCardsInToDrawPile()
    {
        var engine = new DeckEngine();
        var deck = BattleDeck(draw: [], discard: [Instance(1), Instance(2)]);

        engine.ReshuffleDiscardPile(deck);

        Assert.Empty(deck.DiscardPile);
        Assert.Equal(2, deck.DrawPile.Count);
        Assert.Equal([1, 2], deck.DrawPile.Select(card => card.CardId).Order());
    }

    [Fact]
    public void ReshuffleDiscardPile_OnEmptyDiscardPile_LeavesDrawPileUnchanged()
    {
        var engine = new DeckEngine();
        var drawCard = Instance(1);
        var deck = BattleDeck(draw: [drawCard]);

        engine.ReshuffleDiscardPile(deck);

        Assert.Single(deck.DrawPile);
        Assert.Same(drawCard, deck.DrawPile[0]);
        Assert.Empty(deck.DiscardPile);
    }

    [Fact]
    public void AddCard_ReturnsSuccessWhenHandHasExactlyOneSlotFree()
    {
        var handEngine = new HandEngine(new HandRules(maxHandSize: 2));
        var deck = BattleDeck(hand: [Instance(1)]);

        var result = handEngine.AddCard(deck, Instance(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, deck.Hand.Count);
    }

    [Fact]
    public void AddCards_ReturnsHandFullWhenAllCardsDoNotFit()
    {
        var handEngine = new HandEngine(new HandRules(maxHandSize: 3));
        var deck = BattleDeck(hand: [Instance(1), Instance(2)]);

        var result = handEngine.AddCards(deck, [Instance(3), Instance(4)]);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.HAND_FULL, result.ErrorCode);
        Assert.Equal(2, deck.Hand.Count);
    }

    [Fact]
    public void AddCards_ReturnsSuccessWhenExactlyFits()
    {
        var handEngine = new HandEngine(new HandRules(maxHandSize: 3));
        var deck = BattleDeck(hand: [Instance(1)]);

        var result = handEngine.AddCards(deck, [Instance(2), Instance(3)]);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, deck.Hand.Count);
    }

    [Fact]
    public void GetHandSize_ReturnsCurrentHandCount()
    {
        var handEngine = new HandEngine(new HandRules(5));
        var deck = BattleDeck(hand: [Instance(1), Instance(2)]);

        Assert.Equal(2, handEngine.GetHandSize(deck));
    }

    [Fact]
    public void FindCard_LocatesCardByInstanceId()
    {
        var handEngine = new HandEngine(new HandRules(5));
        var held = Instance(1);
        var deck = BattleDeck(hand: [held]);

        var found = handEngine.FindCard(deck, held.InstanceId);

        Assert.Same(held, found);
    }

    [Fact]
    public void FindCardByDefinitionId_LocatesFirstMatchingCard()
    {
        var handEngine = new HandEngine(new HandRules(5));
        var deck = BattleDeck(hand: [Instance(1), Instance(2), Instance(1)]);

        var found = handEngine.FindCardByDefinitionId(deck, cardId: 2);

        Assert.NotNull(found);
        Assert.Equal(2, found.CardId);
    }

    private static Card Card(int id) => new() { Id = id, Name = $"Card {id}" };
    private static CardInstance Instance(int id) => new() { CardId = id, Card = Card(id) };

    private static BattleDeck BattleDeck(
        IList<CardInstance>? draw = null,
        IList<CardInstance>? discard = null,
        IList<CardInstance>? hand = null) => new()
    {
        DrawPile = draw ?? new List<CardInstance>(),
        DiscardPile = discard ?? new List<CardInstance>(),
        Hand = hand ?? new List<CardInstance>()
    };
}
