using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.EnemyActions;

/// <summary>
/// Selects an enemy action without executing its combat effects.
/// </summary>
public interface IEnemyActionSelector
{
    EngineResult<EnemyAction> SelectAction(BattleContext battleContext);
}
