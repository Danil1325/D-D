using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Models;

namespace DnDGame.Domain.Engine.Cards;

/// <summary>
/// Contract for resolving a card played during a battle.
/// </summary>
public interface ICardEngine
{
    EngineResult<BattleState> PlayCard(BattleContext battleContext, CardInstance card);
}
