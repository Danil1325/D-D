namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Configurable thresholds used to evaluate D20 critical outcomes.
/// </summary>
public sealed record CriticalRules(
    int CriticalHitThreshold = 20,
    int CriticalMissThreshold = 1);
