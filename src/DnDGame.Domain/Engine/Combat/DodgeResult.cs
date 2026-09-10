namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Immutable result of a dodge calculation.
/// </summary>
public sealed record DodgeResult(DodgeOutcome Outcome)
{
    public bool IsDodge => Outcome == DodgeOutcome.Dodge;
}
