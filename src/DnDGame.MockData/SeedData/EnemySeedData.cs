using DnDGame.Domain.Entities.Enemies;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds all 21 enemies (7 families x 3 tiers) confirmed from the reference images.
/// Stats escalate per tier within each family; exact values are placeholders (TBD,
/// same as the race attribute ranges) but internally consistent — roughly 1.5x per
/// tier step, with each family's baseline reflecting its relative danger.
///
/// ImagePath values match the actual project asset filenames exactly, including
/// inconsistent casing/spacing between the display Name and the file itself (e.g.
/// "Lord Goblin" -> "Lordgoblin.png", "Warrior Troll" -> "war_troll.png") — these
/// are the real filenames, not a naming-convention guess.
/// </summary>
internal static class EnemySeedData
{
    public static void Seed(InMemoryGameDataStore store)
    {
        store.Enemies.AddRange(new[]
        {
            // Skeleton family
            new Enemy { Id = 1, Name = "Skeleton", Family = EnemyFamily.Skeleton, Tier = EnemyTier.Base, Description = "A reanimated warrior, slow but relentless.", ImagePath = "enemies/Skeleton.png", Health = 10, Defense = 8, AttackBonus = 2, DamageAmount = 3, XpReward = 10, SpecialAbilityText = "None." },
            new Enemy { Id = 2, Name = "Skeleton Knight", Family = EnemyFamily.Skeleton, Tier = EnemyTier.Evolved, Description = "A fallen knight, still bound to its blade and shield.", ImagePath = "enemies/Skeleton_Knight.png", Health = 16, Defense = 11, AttackBonus = 4, DamageAmount = 5, XpReward = 20, SpecialAbilityText = "Fights with disciplined, armored precision." },
            new Enemy { Id = 3, Name = "The Lich", Family = EnemyFamily.Skeleton, Tier = EnemyTier.Elite, Description = "An undead sorcerer-king, master of the crypt.", ImagePath = "enemies/The_Lich.png", Health = 26, Defense = 14, AttackBonus = 7, DamageAmount = 9, XpReward = 45, SpecialAbilityText = "Said to command lesser undead — not mechanically implemented yet." },

            // Goblin family
            new Enemy { Id = 4, Name = "Goblin", Family = EnemyFamily.Goblin, Tier = EnemyTier.Base, Description = "A small, vicious raider.", ImagePath = "enemies/Goblin.png", Health = 8, Defense = 7, AttackBonus = 2, DamageAmount = 2, XpReward = 8, SpecialAbilityText = "None." },
            new Enemy { Id = 5, Name = "Hobgoblin", Family = EnemyFamily.Goblin, Tier = EnemyTier.Evolved, Description = "A disciplined goblin soldier.", ImagePath = "enemies/Hobgoblin.png", Health = 13, Defense = 9, AttackBonus = 4, DamageAmount = 4, XpReward = 16, SpecialAbilityText = "Fights in organized formation." },
            new Enemy { Id = 6, Name = "Lord Goblin", Family = EnemyFamily.Goblin, Tier = EnemyTier.Elite, Description = "The crowned ruler of a goblin warband.", ImagePath = "enemies/Lordgoblin.png", Health = 22, Defense = 12, AttackBonus = 6, DamageAmount = 7, XpReward = 35, SpecialAbilityText = "Commands nearby goblins — not mechanically implemented yet." },

            // Slime family
            new Enemy { Id = 7, Name = "Slime", Family = EnemyFamily.Slime, Tier = EnemyTier.Base, Description = "A small, gelatinous creature.", ImagePath = "enemies/slime.png", Health = 6, Defense = 6, AttackBonus = 1, DamageAmount = 2, XpReward = 5, SpecialAbilityText = "None." },
            new Enemy { Id = 8, Name = "Great Slime", Family = EnemyFamily.Slime, Tier = EnemyTier.Evolved, Description = "A larger, hungrier slime.", ImagePath = "enemies/great_slime.png", Health = 12, Defense = 8, AttackBonus = 3, DamageAmount = 4, XpReward = 12, SpecialAbilityText = "None." },
            new Enemy { Id = 9, Name = "King Slime", Family = EnemyFamily.Slime, Tier = EnemyTier.Elite, Description = "An enormous slime, crowned and unnervingly cheerful.", ImagePath = "enemies/king_slime.png", Health = 20, Defense = 10, AttackBonus = 5, DamageAmount = 6, XpReward = 28, SpecialAbilityText = "None." },

            // Troll family
            new Enemy { Id = 10, Name = "Troll", Family = EnemyFamily.Troll, Tier = EnemyTier.Base, Description = "A brutish, hardy creature.", ImagePath = "enemies/troll.png", Health = 14, Defense = 9, AttackBonus = 3, DamageAmount = 4, XpReward = 15, SpecialAbilityText = "Rumored to regenerate wounds — not mechanically implemented yet." },
            new Enemy { Id = 11, Name = "Warrior Troll", Family = EnemyFamily.Troll, Tier = EnemyTier.Evolved, Description = "A troll armed and armored for war.", ImagePath = "enemies/war_troll.png", Health = 22, Defense = 12, AttackBonus = 5, DamageAmount = 7, XpReward = 28, SpecialAbilityText = "Rumored to regenerate wounds — not mechanically implemented yet." },
            new Enemy { Id = 12, Name = "Troll King", Family = EnemyFamily.Troll, Tier = EnemyTier.Elite, Description = "The massive, crowned ruler of the trolls.", ImagePath = "enemies/troll_king.png", Health = 34, Defense = 15, AttackBonus = 8, DamageAmount = 11, XpReward = 55, SpecialAbilityText = "Rumored to regenerate wounds — not mechanically implemented yet." },

            // Chimera family
            new Enemy { Id = 13, Name = "Chimera", Family = EnemyFamily.Chimera, Tier = EnemyTier.Base, Description = "A goat, a lion, and a serpent fused into one beast.", ImagePath = "enemies/chimera.png", Health = 16, Defense = 10, AttackBonus = 4, DamageAmount = 5, XpReward = 20, SpecialAbilityText = "None." },
            new Enemy { Id = 14, Name = "Great Chimera", Family = EnemyFamily.Chimera, Tier = EnemyTier.Evolved, Description = "A larger, winged chimera.", ImagePath = "enemies/great_chimera.png", Health = 25, Defense = 13, AttackBonus = 6, DamageAmount = 8, XpReward = 38, SpecialAbilityText = "None." },
            new Enemy { Id = 15, Name = "Divine Chimera", Family = EnemyFamily.Chimera, Tier = EnemyTier.Elite, Description = "A radiant, terrifying chimera said to be touched by the gods.", ImagePath = "enemies/divine_chimera.png", Health = 38, Defense = 16, AttackBonus = 9, DamageAmount = 12, XpReward = 65, SpecialAbilityText = "None." },

            // Demon family
            new Enemy { Id = 16, Name = "Demon", Family = EnemyFamily.Demon, Tier = EnemyTier.Base, Description = "A lesser fiend, clawed and quick to anger.", ImagePath = "enemies/demon.png", Health = 18, Defense = 11, AttackBonus = 4, DamageAmount = 5, XpReward = 22, SpecialAbilityText = "None." },
            new Enemy { Id = 17, Name = "Greater Demon", Family = EnemyFamily.Demon, Tier = EnemyTier.Evolved, Description = "A stronger, more cunning fiend.", ImagePath = "enemies/greater_demon.png", Health = 28, Defense = 14, AttackBonus = 7, DamageAmount = 9, XpReward = 42, SpecialAbilityText = "None." },
            new Enemy { Id = 18, Name = "Demon Lord", Family = EnemyFamily.Demon, Tier = EnemyTier.Elite, Description = "A crowned ruler of fiends, radiating malice.", ImagePath = "enemies/demon_lord.png", Health = 42, Defense = 18, AttackBonus = 10, DamageAmount = 14, XpReward = 75, SpecialAbilityText = "None." },

            // Wraith family
            new Enemy { Id = 19, Name = "Phantom", Family = EnemyFamily.Wraith, Tier = EnemyTier.Base, Description = "A restless, half-formed spirit.", ImagePath = "enemies/Phantom.png", Health = 12, Defense = 10, AttackBonus = 3, DamageAmount = 4, XpReward = 14, SpecialAbilityText = "None." },
            new Enemy { Id = 20, Name = "Wraith", Family = EnemyFamily.Wraith, Tier = EnemyTier.Evolved, Description = "A malevolent spirit, cold to the touch.", ImagePath = "enemies/Wraith.png", Health = 19, Defense = 13, AttackBonus = 5, DamageAmount = 7, XpReward = 26, SpecialAbilityText = "None." },
            new Enemy { Id = 21, Name = "Dread Wraith", Family = EnemyFamily.Wraith, Tier = EnemyTier.Elite, Description = "A crowned, ancient horror wreathed in dark flame.", ImagePath = "enemies/Dread_Wraith.png", Health = 30, Defense = 16, AttackBonus = 8, DamageAmount = 10, XpReward = 50, SpecialAbilityText = "None." }
        });
    }
}
