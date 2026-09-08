using DnDGame.Domain.Common;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Enemies;

/// <summary>
/// One specific enemy (e.g. "Hobgoblin"). All 21 enemies (7 families x 3 tiers) are
/// rows of this single class — there is no per-monster C# type, per your instruction.
/// </summary>
public class Enemy : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public EnemyFamily Family { get; set; }
    public EnemyTier Tier { get; set; }

    public string Description { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;

    public int Health { get; set; }

    /// <summary>The difficulty value a player's attack roll must meet or beat to hit this enemy.</summary>
    public int Defense { get; set; }

    /// <summary>Added to this enemy's 2d6 roll when it attacks. Enemies don't have the 5 player attributes.</summary>
    public int AttackBonus { get; set; }

    /// <summary>Flat damage dealt by this enemy on a successful hit. No damage dice.</summary>
    public int DamageAmount { get; set; }

    public int XpReward { get; set; }

    /// <summary>
    /// Free-text description of anything unique (e.g. "regenerates each round").
    /// Not every special ability described here is mechanically implemented yet —
    /// some are flavor only until a later phase gives them real behavior.
    /// </summary>
    public string SpecialAbilityText { get; set; } = string.Empty;
}
