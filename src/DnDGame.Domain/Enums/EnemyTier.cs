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
    Elite = 3
}
