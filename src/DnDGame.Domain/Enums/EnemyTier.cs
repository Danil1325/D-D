namespace DnDGame.Domain.Enums;

/// <summary>
/// The 3 evolution stages within an enemy family (e.g. Goblin -> Hobgoblin -> Lord Goblin).
/// Explicit numeric values so "is this tier stronger than that one" is a plain integer
/// comparison if it's ever needed.
/// </summary>
public enum EnemyTier
{
    Base = 1,
    Evolved = 2,
    Elite = 3,

    /// <summary>
    /// Unique, named campaign bosses that sit above a family's normal Elite entry
    /// (e.g. Karnyx above Divine Chimera). Added for BACK-LOC-05 so these extra
    /// entries don't collide with the existing one-enemy-per-(Family,Tier) row for
    /// Base/Evolved/Elite that other seed data (e.g. ScenarioSideQuestSeedData)
    /// relies on.
    /// </summary>
    Boss = 4
}
