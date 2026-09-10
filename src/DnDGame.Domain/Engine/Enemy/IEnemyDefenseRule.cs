using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.EnemyActions;

/// <summary>
/// Supplies the block amount granted by an enemy Defend action.
/// </summary>
public interface IEnemyDefenseRule
{
    EngineResult<int> CalculateBlock(BattleContext battleContext);
}
