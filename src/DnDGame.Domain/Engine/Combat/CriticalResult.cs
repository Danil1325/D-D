using DnDGame.Domain.Engine.Dice;

namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Immutable critical outcome associated with a backend-generated dice result.
/// </summary>
public sealed record CriticalResult(CriticalOutcome Outcome, DiceResult DiceResult);
