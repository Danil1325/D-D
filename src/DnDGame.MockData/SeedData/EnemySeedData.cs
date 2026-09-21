using DnDGame.Domain.Entities.Enemies;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds the 21 enemies (7 families x 3 tiers) confirmed from the reference images,
/// plus 6 named campaign bosses (Ids 22-27) added for BACK-LOC-05's location encounter
/// pools. Stats for the original 21 escalate per tier within each family; exact values
/// are placeholders (TBD, same as the race attribute ranges) but internally consistent
/// — roughly 1.5x per tier step, with each family's baseline reflecting its relative
/// danger. The 6 named bosses are separate, unique placeholders (see their own
/// SpecialAbilityText) not yet balanced against that progression.
///
/// ImagePath values match the actual project asset filenames exactly, including
/// inconsistent casing/spacing between the display Name and the file itself (e.g.
/// "Lord Goblin" -> "Lordgoblin.png", "Warrior Troll" -> "war_troll.png") — these
/// are the real filenames, not a naming-convention guess. The 6 boss ImagePath values
/// are guesses following that same convention and have not been verified against
/// real asset files.
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
            new Enemy { Id = 21, Name = "Dread Wraith", Family = EnemyFamily.Wraith, Tier = EnemyTier.Elite, Description = "A crowned, ancient horror wreathed in dark flame.", ImagePath = "enemies/Dread_Wraith.png", Health = 30, Defense = 16, AttackBonus = 8, DamageAmount = 10, XpReward = 50, SpecialAbilityText = "None." },

            // Named campaign bosses (BACK-LOC-05) — referenced by LocationEncounterSeedData as
            // Boss-tier encounters. PLACEHOLDER stats only: these are unique, above-Elite bosses
            // with no reference card, so values are rough extrapolations above their family's
            // existing Elite entry. To be finalized later — do not treat as balanced.
            new Enemy { Id = 22, Name = "The Herald", Family = EnemyFamily.Wraith, Tier = EnemyTier.Boss, Description = "A spectral herald of the coming ruin. Placeholder lore.", ImagePath = "enemies/The_Herald.png", Health = 55, Defense = 20, AttackBonus = 11, DamageAmount = 15, XpReward = 100, SpecialAbilityText = "PLACEHOLDER — stats/ability to be finalized." },
            new Enemy { Id = 23, Name = "Vharruk", Family = EnemyFamily.Demon, Tier = EnemyTier.Boss, Description = "A demonic power behind the ruin. Placeholder lore.", ImagePath = "enemies/Vharruk.png", Health = 60, Defense = 21, AttackBonus = 12, DamageAmount = 16, XpReward = 110, SpecialAbilityText = "PLACEHOLDER — stats/ability to be finalized." },
            new Enemy { Id = 24, Name = "Karnyx", Family = EnemyFamily.Chimera, Tier = EnemyTier.Boss, Description = "A monstrous chimeric warlord. Placeholder lore.", ImagePath = "enemies/Karnyx.png", Health = 50, Defense = 19, AttackBonus = 10, DamageAmount = 13, XpReward = 90, SpecialAbilityText = "PLACEHOLDER — stats/ability to be finalized." },
            new Enemy { Id = 25, Name = "Nerath-Dur the Lich", Family = EnemyFamily.Skeleton, Tier = EnemyTier.Boss, Description = "An ancient lich-king of the Bone Peaks. Placeholder lore.", ImagePath = "enemies/Nerath_Dur.png", Health = 48, Defense = 18, AttackBonus = 10, DamageAmount = 13, XpReward = 90, SpecialAbilityText = "PLACEHOLDER — stats/ability to be finalized." },
            new Enemy { Id = 26, Name = "Grommash-Vurr the Troll King", Family = EnemyFamily.Troll, Tier = EnemyTier.Boss, Description = "A rival troll king ruling the Bone Peaks. Placeholder lore.", ImagePath = "enemies/Grommash_Vurr.png", Health = 52, Defense = 19, AttackBonus = 11, DamageAmount = 14, XpReward = 95, SpecialAbilityText = "PLACEHOLDER — stats/ability to be finalized." },
            new Enemy { Id = 27, Name = "Greater Demon Mayor", Family = EnemyFamily.Demon, Tier = EnemyTier.Boss, Description = "Oakheaven's corrupted mayor, transformed by fiendish influence. Placeholder lore.", ImagePath = "enemies/Greater_Demon_Mayor.png", Health = 45, Defense = 17, AttackBonus = 9, DamageAmount = 12, XpReward = 80, SpecialAbilityText = "PLACEHOLDER — stats/ability to be finalized." }
        });
    }
}
