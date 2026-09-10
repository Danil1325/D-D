using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Models;
using Xunit;

namespace DnDGame.Tests.Engine.Battle;

public class BattleLogTests
{
    [Fact]
    public void EntryIsAddedWithBattleTurnAndTimestamp()
    {
        var battleState = new BattleState { TurnNumber = 3 };
        var writer = new BattleLogWriter();
        var entry = new BattleLogEntry(
            turnNumber: battleState.TurnNumber,
            actor: "Enemy",
            action: "EnemyAttack",
            damage: 5);

        writer.Write(battleState, entry);

        Assert.Single(battleState.BattleLog);
        Assert.Same(entry, battleState.BattleLog[0]);
        Assert.Equal(3, entry.TurnNumber);
        Assert.NotEqual(default, entry.Timestamp);
    }

    [Fact]
    public void BattleLogIsNeverNullForNewBattleState()
    {
        var battleState = new BattleState();

        Assert.NotNull(battleState.BattleLog);
        Assert.Empty(battleState.BattleLog);
    }
}
