using DnDGame.BusinessLayer.Dtos.Dice;
using DnDGame.Domain.Engine.Models;

namespace DnDGame.BusinessLayer.Dtos.Battles;

/// <summary>
/// Response shape for one battle-log entry — mirrors
/// DnDGame.Domain.Engine.Models.BattleLogEntry. Reuses DiceResultDto for the
/// dice-roll field instead of duplicating it.
/// </summary>
public class BattleLogEntryDto
{
    public Guid Id { get; init; }
    public int TurnNumber { get; init; }
    public string Actor { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string? Card { get; init; }
    public DiceResultDto? DiceResult { get; init; }
    public int? Damage { get; init; }
    public int? Healing { get; init; }
    public IReadOnlyList<string> Effects { get; init; } = Array.Empty<string>();
    public string? Result { get; init; }
    public DateTimeOffset Timestamp { get; init; }

    public static BattleLogEntryDto FromDomain(BattleLogEntry entry) => new()
    {
        Id = entry.Id,
        TurnNumber = entry.TurnNumber,
        Actor = entry.Actor,
        Action = entry.Action,
        Card = entry.Card,
        DiceResult = entry.DiceResult is null ? null : DiceResultDto.FromDomain(entry.DiceResult),
        Damage = entry.Damage,
        Healing = entry.Healing,
        Effects = entry.Effects,
        Result = entry.Result,
        Timestamp = entry.Timestamp
    };
}
