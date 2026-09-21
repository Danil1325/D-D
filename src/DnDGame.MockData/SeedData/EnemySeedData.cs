using DnDGame.Domain.Entities.Enemies;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds the 21 enemies (7 families x 3 tiers) confirmed from the reference images,
/// plus 6 named campaign bosses (Ids 22-27) added for BACK-LOC-05's location encounter
/// pools. Stats for the original 21 escalate per tier within each family;
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
            // Boss-tier encounters.
            new Enemy
            {
                Id = 22,
                Name = "The Herald",
                Family = EnemyFamily.Wraith,
                Tier = EnemyTier.Boss,
                Description = "A spectral herald who announces Vharruk's return and leads the assault on Hero's Overlook.",
                ImagePath = "enemies/The_Herald.png",
                Health = 55,
                Defense = 20,
                AttackBonus = 11,
                DamageAmount = 15,
                XpReward = 100,
                SpecialAbilityText = "Spectral Summons — Summons one Phantom during battle and can only be reliably damaged by magic, light, silver, or blessed weapons."
            },
            
            new Enemy
            {
                Id = 23,
                Name = "Vharruk",
                Family = EnemyFamily.Demon,
                Tier = EnemyTier.Boss,
                Description = "The Demon Lord known as the King of Darkness and the final enemy connected to the Crown of Ash.",
                ImagePath = "enemies/Vharruk.png",
                Health = 160,
                Defense = 26,
                AttackBonus = 13,
                DamageAmount = 17,
                XpReward = 300,
                SpecialAbilityText = "Three Phases — Returns with 180 Health in phase two and 200 Health in phase three. He cannot be permanently defeated while the Crown remains intact."
            },
            
            new Enemy
            {
                Id = 24,
                Name = "Karnyx",
                Family = EnemyFamily.Demon,
                Tier = EnemyTier.Boss,
                Description = "A twice-burned Greater Demon guarding the sealed chamber beneath Ashtonia.",
                ImagePath = "enemies/Karnyx.png",
                Health = 65,
                Defense = 18,
                AttackBonus = 10,
                DamageAmount = 14,
                XpReward = 120,
                SpecialAbilityText = "Twice-Burned Regeneration — Restores 5 Health each round and can summon two Demons once per battle. Consecration or fire stops the regeneration."
            },
            
            new Enemy
            {
                Id = 25,
                Name = "Nerath-Dur the Lich",
                Family = EnemyFamily.Skeleton,
                Tier = EnemyTier.Boss,
                Description = "The undead ruler of Karag-Dur who guards the Third Crown Fragment and refuses to let the dead leave his halls.",
                ImagePath = "enemies/Nerath_Dur_The_Lich.png",
                Health = 90,
                Defense = 22,
                AttackBonus = 10,
                DamageAmount = 12,
                XpReward = 160,
                SpecialAbilityText = "Hidden Phylactery — Cannot be permanently defeated until his phylactery is destroyed. The encounter can also be resolved through negotiation or ritual."
            },
            
            new Enemy
            {
                Id = 26,
                Name = "Gromash-Vurr the Troll King",
                Family = EnemyFamily.Troll,
                Tier = EnemyTier.Boss,
                Description = "The Troll King who seized the great forge of Karag-Dur and seeks a war worthy of being remembered.",
                ImagePath = "enemies/Gromash_Vurr_The_Troll_King.png",
                Health = 110,
                Defense = 23,
                AttackBonus = 12,
                DamageAmount = 18,
                XpReward = 160,
                SpecialAbilityText = "Royal Regeneration — Restores 6 Health each round and empowers his attacks with magic. Fire, acid, or magical debuffs stop the regeneration."
            },
            
            new Enemy
            {
                Id = 27,
                Name = "Greater Demon Mayor",
                Family = EnemyFamily.Demon,
                Tier = EnemyTier.Boss,
                Description = "The Greater Demon controlling Oakheaven during the demonic occupation and the main threat to the surviving townspeople.",
                ImagePath = "enemies/Greater_Demon_Mayor.png",
                Health = 70,
                Defense = 19,
                AttackBonus = 10,
                DamageAmount = 14,
                XpReward = 120,
                SpecialAbilityText = "Demonic Authority — Summons two Demons once per battle and regenerates 5 Health each round unless affected by consecration or fire.",
            },

            // Added per explicit user request (BACK-LOC-06) so the "Kregg is not an enemy
            // once goblinAlliance is true" rule has a real Enemy row to exclude. Tier is
            // EnemyTier.Boss (not Elite, despite being a "Goblin Lord") to avoid colliding
            // with Lord Goblin (Id 6), which already holds Goblin's Elite slot — see the
            // EnemyTier.Boss doc comment for why the (Family, Tier) pairing must stay unique.
            new Enemy
            {
                Id = 28,
                Name = "Kregg the Sundered",
                Family = EnemyFamily.Goblin,
                Tier = EnemyTier.Boss,
                Description = "A proud Goblin Lord commanding the goblin forces around Oakheaven. He may become an ally if the player forms the goblin alliance.",
                ImagePath = "enemies/Kregg_The_Sundered.png",
                Health = 55,
                Defense = 16,
                AttackBonus = 8,
                DamageAmount = 10,
                XpReward = 90,
                SpecialAbilityText = "Warband Commander — Summons two Goblins once per battle. Kregg does not appear as an enemy when the goblin alliance is active."
            }
        });
    }
}
