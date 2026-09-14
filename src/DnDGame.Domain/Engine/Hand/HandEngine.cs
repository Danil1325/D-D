using DnDGame.Domain.Engine.Battle;

namespace DnDGame.Domain.Engine.Hand;

/// <summary>
/// Moves every card in the player's hand back to the discard pile at the end of
/// the player turn.
/// </summary>
public sealed class HandEngine : IHandEngine
{
    public void DiscardHand(BattleState battleState)
    {
        ArgumentNullException.ThrowIfNull(battleState);

        foreach (var card in battleState.Hand)
        {
            battleState.DiscardPile.Add(card);
        }

        battleState.Hand.Clear();
    }
}