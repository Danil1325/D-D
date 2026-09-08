namespace DnDGame.Domain.Enums;

/// <summary>
/// The 7 enemy families confirmed from the reference cards. Each family has 3 tiers
/// (see EnemyTier). Individual enemies are DATA — rows in the Enemy table seeded by
/// MockData/DataAccessLayer — not one C# class per monster.
/// </summary>
public enum EnemyFamily
{
    Skeleton,
    Goblin,
    Slime,
    Troll,
    Chimera,
    Demon,
    Wraith
}
