namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Describes the result of a damage calculation before any health is changed.
/// </summary>
public sealed record DamageResult(
    int BaseDamage,
    int BlockedDamage,
    int FinalDamage,
    int RemainingBlock);
