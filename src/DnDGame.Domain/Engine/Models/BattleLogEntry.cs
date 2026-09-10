namespace DnDGame.Domain.Engine.Models;

/// <summary>
/// Minimal immutable record of an event that occurred during a battle.
/// </summary>
public sealed record BattleLogEntry(DateTime OccurredAt, string Message);
