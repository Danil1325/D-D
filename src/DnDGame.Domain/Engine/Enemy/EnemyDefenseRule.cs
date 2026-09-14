using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.EnemyActions;

/// <summary>
/// A Defend action grants the enemy block equal to half its Defense stat
/// (rounded down, minimum 1).
/// </summary>
public sealed class EnemyDefenseRule : IEnemyDefenseRule
{
    public EngineResult<int> CalculateBlock(BattleContext battleContext)
    {
        ArgumentNullException.ThrowIfNull(battleContext);

        var block = Math.Max(1, battleContext.Enemy.Defense / 2);
        return EngineResult<int>.Ok(block);
    }
}