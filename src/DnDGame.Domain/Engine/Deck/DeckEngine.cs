using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Dice;
using DnDGame.Domain.Entities.Cards;

namespace DnDGame.Domain.Engine.Deck;

/// <summary>
/// Draws the player's opening hand at the start of each player turn. When the
/// draw pile is exhausted the discard pile is reshuffled back in, mirroring the
/// card-battle convention that the deck never runs out.
/// </summary>
public sealed class DeckEngine : IDeckEngine
{
    /// <summary>Number of cards drawn at the start of a player turn.</summary>
    public const int CardsDrawnPerTurn = 5;

    private readonly IRandomNumberSource? _randomNumberSource;

    public DeckEngine(IRandomNumberSource? randomNumberSource = null)
    {
        _randomNumberSource = randomNumberSource;
    }

    public void DrawCardsForPlayerTurn(BattleState battleState)
    {
        ArgumentNullException.ThrowIfNull(battleState);

        for (var i = 0; i < CardsDrawnPerTurn; i++)
        {
            if (DrawCard(battleState) is null)
            {
                break;
            }
        }
    }

    private CardInstance? DrawCard(BattleState battleState)
    {
        if (battleState.DrawPile.Count == 0)
        {
            ReshuffleDiscardPile(battleState);
        }

        if (battleState.DrawPile.Count == 0)
        {
            return null;
        }

        var drawnCard = battleState.DrawPile[^1];
        battleState.DrawPile.RemoveAt(battleState.DrawPile.Count - 1);
        battleState.Hand.Add(drawnCard);
        return drawnCard;
    }

    private void ReshuffleDiscardPile(BattleState battleState)
    {
        while (battleState.DiscardPile.Count > 0)
        {
            var card = battleState.DiscardPile[0];
            battleState.DiscardPile.RemoveAt(0);
            battleState.DrawPile.Add(card);
        }

        Shuffle(battleState.DrawPile);
    }

    private void Shuffle(IList<CardInstance> cards)
    {
        for (var i = cards.Count - 1; i > 0; i--)
        {
            var randomIndex = _randomNumberSource?.Next(0, i + 1) ?? Random.Shared.Next(0, i + 1);
            (cards[i], cards[randomIndex]) = (cards[randomIndex], cards[i]);
        }
    }
}