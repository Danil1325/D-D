using DnDGame.Domain.Engine.Enums;

namespace DnDGame.Domain.Engine.Battle;

/// <summary>
/// Immutable-style result data produced after a battle reaches a terminal state.
/// </summary>
public class BattleResult
{
    public Guid BattleId { get; init; }

    public BattleStatus Status { get; init; }

    public required BattleState FinalState { get; init; }
}
