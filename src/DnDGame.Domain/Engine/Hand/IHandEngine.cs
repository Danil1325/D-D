using DnDGame.Domain.Engine.Battle;

namespace DnDGame.Domain.Engine.Hand;

/// <summary>
/// Contract for hand operations coordinated at turn boundaries.
/// </summary>
public interface IHandEngine
{
    void DiscardHand(BattleState battleState);
}
