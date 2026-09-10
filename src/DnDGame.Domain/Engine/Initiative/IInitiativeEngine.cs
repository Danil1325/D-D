using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Enums;

namespace DnDGame.Domain.Engine.Initiative;

/// <summary>
/// Determines which combatant starts a battle.
/// </summary>
public interface IInitiativeEngine
{
    TurnType DetermineFirstTurn(BattleContext battleContext);
}
