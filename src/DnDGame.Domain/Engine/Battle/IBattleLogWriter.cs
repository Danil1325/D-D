using DnDGame.Domain.Engine.Models;

namespace DnDGame.Domain.Engine.Battle;

/// <summary>
/// Appends battle-history entries to a battle state.
/// </summary>
public interface IBattleLogWriter
{
    void Write(BattleState battleState, BattleLogEntry entry);
}
