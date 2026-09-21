namespace DnDGame.Domain.Enums;

/// <summary>
/// How dangerous an encounter slot at a location is. Distinct from EnemyTier (which
/// tracks a single enemy family's own Base/Evolved/Elite progression) — this tracks
/// the encounter itself, e.g. a Base-tier Enemy can still appear in a location's
/// Elite encounter pool.
/// </summary>
public enum EncounterTier
{
    Normal,
    Elite,
    Boss
}
