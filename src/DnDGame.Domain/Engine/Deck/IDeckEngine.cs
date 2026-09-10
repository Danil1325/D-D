using DnDGame.Domain.Engine.Battle;

namespace DnDGame.Domain.Engine.Deck;

/// <summary>
/// Contract for deck operations coordinated at turn boundaries.
/// </summary>
public interface IDeckEngine
{
    void DrawCardsForPlayerTurn(BattleState battleState);
}
