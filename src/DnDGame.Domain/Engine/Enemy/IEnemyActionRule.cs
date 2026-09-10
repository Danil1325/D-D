using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.EnemyActions;

/// <summary>
/// Configurable policy used to choose an action for an enemy archetype.
/// </summary>
public interface IEnemyActionRule
{
    EngineResult<EnemyAction> SelectAction(BattleContext battleContext);
}
