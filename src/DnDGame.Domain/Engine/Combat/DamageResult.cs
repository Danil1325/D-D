namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Describes each stage of a damage calculation before any health is changed.
/// </summary>
public sealed record DamageResult(
    int RawDamage,
    int ModifiedDamage,
    int DamageAfterDefense,
    int BlockedDamage,
    int FinalDamage,
    int RemainingBlock);
