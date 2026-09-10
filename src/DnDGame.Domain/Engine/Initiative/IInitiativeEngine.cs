using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.Initiative;

/// <summary>
/// Determines which combatant starts a battle.
/// </summary>
public interface IInitiativeEngine
{
    EngineResult<InitiativeResult> DetermineFirstTurn(BattleContext battleContext);
}
