namespace DnDGame.Domain.Enums;

/// <summary>
/// Defines the target type for a card's effect.
/// </summary>
public enum TargetType
{
    Self = 0,
    SingleEnemy = 1,
    SingleAlly = 2,
    AllEnemies = 3,
    AllAllies = 4,
    RandomTarget = 5,
    None = 6
}
