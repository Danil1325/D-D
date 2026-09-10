using DnDGame.Domain.Engine.Models;

namespace DnDGame.Domain.Engine.Battle;

/// <summary>
/// Domain-only writer for the battle history held by BattleState.
/// </summary>
public sealed class BattleLogWriter : IBattleLogWriter
{
    public void Write(BattleState battleState, BattleLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(battleState);
        ArgumentNullException.ThrowIfNull(entry);

        battleState.BattleLog.Add(entry);
    }
}
