using DnDGame.Domain.Engine.Dice;

namespace DnDGame.Domain.Engine.Models;

/// <summary>
/// Immutable history entry for a meaningful battle event. This is game history,
/// not application or HTTP logging.
/// </summary>
public sealed class BattleLogEntry
{
    public BattleLogEntry(
        int turnNumber,
        string actor,
        string action,
        string? card = null,
        DiceResult? diceResult = null,
        int? damage = null,
        int? healing = null,
        IEnumerable<string>? effects = null,
        string? result = null,
        DateTimeOffset? timestamp = null)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(action);

        Id = Guid.NewGuid();
        TurnNumber = turnNumber;
        Actor = actor;
        Action = action;
        Card = card;
        DiceResult = diceResult;
        Damage = damage;
        Healing = healing;
        Effects = (effects ?? Array.Empty<string>()).ToArray();
        Result = result;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
    }

    public Guid Id { get; }
    public int TurnNumber { get; }
    public string Actor { get; }
    public string Action { get; }
    public string? Card { get; }
    public DiceResult? DiceResult { get; }
    public int? Damage { get; }
    public int? Healing { get; }
    public IReadOnlyList<string> Effects { get; }
    public string? Result { get; }
    public DateTimeOffset Timestamp { get; }
}
