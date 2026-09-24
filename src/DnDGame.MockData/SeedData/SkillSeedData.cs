using DnDGame.Domain.Entities.Skills;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds the skill-tree catalog: one row per skill across all 16 (Race, Class)
/// combinations, transcribed from the frontend's
/// src/features/skill-tree/data/skillTreeData.ts. Code values match the
/// frontend's skill ids exactly (e.g. "human-mage-arcane-adaptation") so both
/// sides stay keyed the same way.
///
/// Cost is flat (the frontend's STANDARD_SKILL_COST, 1 skill point here) — the
/// frontend's own skillTreeData.ts marks RequiredLevel/PrerequisiteSkillId as
/// "provisional... until official rules exist", so this catalog does not invent
/// them; see SkillService's remarks.
///
/// Race ids: Human=1, Elf=2, Orc=3, Dwarf=4 (RaceSeedData).
/// Class ids: Healer=1, Warrior=2, Magician=3, Bard=4 (ClassSeedData) — the
/// frontend's "Mage" is this catalog's "Magician" (ClassId 3); only the naming
/// differs, it's the same class.
/// </summary>
internal static class SkillSeedData
{
    private const int Human = 1;
    private const int Elf = 2;
    private const int Orc = 3;
    private const int Dwarf = 4;

    private const int Healer = 1;
    private const int Warrior = 2;
    private const int Magician = 3;
    private const int Bard = 4;

    private const int StandardCost = 1;

    public static void Seed(InMemoryGameDataStore store)
    {
        var nextId = 1;
        var skills = new List<SkillDefinition>();

        void AddBuild(int raceId, int classId, params (string Code, string Name, string IconKey, SkillCategory Category, string Description)[] rows)
        {
            foreach (var row in rows)
            {
                skills.Add(new SkillDefinition
                {
                    Id = nextId++,
                    Code = row.Code,
                    RaceId = raceId,
                    ClassId = classId,
                    Name = row.Name,
                    Description = row.Description,
                    IconKey = row.IconKey,
                    Category = row.Category,
                    Cost = StandardCost
                });
            }
        }

        AddBuild(Human, Magician,
            ("human-mage-arcane-adaptation", "Arcane Adaptation", "arcane-star", SkillCategory.Magic, "At the beginning of combat, choose one bonus: +15% Spell Damage, +15% Mana Regen, or +10% Dodge."),
            ("human-mage-quick-study", "Quick Study", "open-book", SkillCategory.Magic, "After using a spell for the first time, the next spell of the same type costs 1 less Mana."),
            ("human-mage-mana-reserve", "Mana Reserve", "mana-drop", SkillCategory.Utility, "Can preserve up to 2 unused Mana for the next turn."),
            ("human-mage-improvised-spell", "Improvised Spell", "mirror", SkillCategory.Magic, "Copies the effect of the last spell used by an enemy at 70% power."),
            ("human-mage-arcane-shield", "Arcane Shield", "ward", SkillCategory.Defense, "Creates a shield equal to 20% of Max HP."),
            ("human-mage-overcharge", "Overcharge", "flame", SkillCategory.Magic, "The next spell deals +50% damage but costs +2 Mana."),
            ("human-mage-master-of-none", "Master of None", "crown", SkillCategory.Magic, "Gain +5% efficiency with all types of magic, but specializations of +25% or higher cannot be obtained."));

        AddBuild(Human, Warrior,
            ("human-warrior-adaptive-stance", "Adaptive Stance", "shield", SkillCategory.Defense, "At the beginning of combat, choose Offensive (+15% Damage) or Defensive (+15% Armor)."),
            ("human-warrior-human-determination", "Human Determination", "ward", SkillCategory.Defense, "The first debuff received in battle lasts 1 turn less."),
            ("human-warrior-tactical-strike", "Tactical Strike", "crossed-swords", SkillCategory.Combat, "Deal +20% damage when attacking an enemy that has already been attacked by an ally during the same turn."),
            ("human-warrior-counterattack", "Counterattack", "crossed-swords", SkillCategory.Combat, "After blocking an attack, automatically counter-attack for 50% damage."),
            ("human-warrior-second-wind", "Second Wind", "claw", SkillCategory.Survival, "Recover 20% HP once per battle."),
            ("human-warrior-battle-experience", "Battle Experience", "open-book", SkillCategory.Utility, "After every 3 hits dealt, gain +5% Attack, stacking up to 3 times."),
            ("human-warrior-last-stand", "Last Stand", "crown", SkillCategory.Survival, "Below 25% HP, gain +30% Damage and +20% Armor."));

        AddBuild(Human, Bard,
            ("human-bard-inspiring-tune", "Inspiring Tune", "staff", SkillCategory.Utility, "An ally gains +15% Damage for 2 turns."),
            ("human-bard-quick-melody", "Quick Melody", "gear", SkillCategory.Utility, "Can use a buff and an attack in the same turn."),
            ("human-bard-encore", "Encore", "mirror", SkillCategory.Utility, "Repeats the last buff used at 50% efficiency."),
            ("human-bard-lucky-performance", "Lucky Performance", "arcane-star", SkillCategory.Utility, "A buff has a 15% chance to have no cooldown."),
            ("human-bard-rally-cry", "Rally Cry", "staff", SkillCategory.Utility, "All allies gain +10% Movement for 1 turn."),
            ("human-bard-versatile-performer", "Versatile Performer", "gear", SkillCategory.Utility, "Can switch melody type between Offensive, Defensive, and Recovery."),
            ("human-bard-jack-of-all-trades", "Jack of All Trades", "crown", SkillCategory.Utility, "+5% to all stats, but cannot exceed the specialization bonuses of other Bards."));

        AddBuild(Human, Healer,
            ("human-healer-healing-touch", "Healing Touch", "ward", SkillCategory.Survival, "Healing on targets below 30% HP is 30% more potent."),
            ("human-healer-first-aid", "First Aid", "gear", SkillCategory.Survival, "Removes Bleeding, Poison, or Burn."),
            ("human-healer-emergency-heal", "Emergency Heal", "ward", SkillCategory.Survival, "Heals an ally for 25% HP."),
            ("human-healer-protective-prayer", "Protective Prayer", "shield", SkillCategory.Defense, "Target takes 20% less damage for 2 turns."),
            ("human-healer-battle-medic", "Battle Medic", "gear", SkillCategory.Utility, "Can heal and move during the same turn."),
            ("human-healer-adaptable-healing", "Adaptable Healing", "ward", SkillCategory.Survival, "Heals more if the target has a debuff, but the base heal is 10% weaker."),
            ("human-healer-second-chance", "Second Chance", "crown", SkillCategory.Survival, "When an ally would die, keeps them at 1 HP and heals them for 15%."));

        AddBuild(Orc, Magician,
            ("orc-mage-blood-magic", "Blood Magic", "mana-drop", SkillCategory.Magic, "Can pay 10% HP instead of 1 Mana."),
            ("orc-mage-brutal-spell", "Brutal Spell", "flame", SkillCategory.Magic, "+20% Spell Damage, but -10% Spell Accuracy."),
            ("orc-mage-rage-casting", "Rage Casting", "flame", SkillCategory.Magic, "Below 40% HP, spells deal +25% damage."),
            ("orc-mage-unstable-magic", "Unstable Magic", "arcane-star", SkillCategory.Magic, "Spells have a 10% chance to deal 50% additional damage."),
            ("orc-mage-forbidden-overload", "Forbidden Overload", "mana-drop", SkillCategory.Magic, "Sacrifice 25% HP so the next spell costs 0 Mana."),
            ("orc-mage-internal-burst", "Infernal Burst", "flame", SkillCategory.Magic, "Area-of-effect magic attack that applies Burning."),
            ("orc-mage-shamans-curse", "Shaman's Curse", "staff", SkillCategory.Magic, "An enemy deals 20% less damage, but cannot be healed for 2 turns."));

        AddBuild(Orc, Warrior,
            ("orc-warrior-blood-rage", "Blood Rage", "claw", SkillCategory.Combat, "Gains +20% Damage when below 50% HP."),
            ("orc-warrior-brutal-strike", "Brutal Strike", "crossed-swords", SkillCategory.Combat, "Attacks ignore 20% Armor."),
            ("orc-warrior-savage-momentum", "Savage Momentum", "claw", SkillCategory.Combat, "Gains +20% Movement for 1 turn after a kill."),
            ("orc-warrior-battle-strike", "Battle Strike", "crossed-swords", SkillCategory.Utility, "Removes Fear and Stun from an ally."),
            ("orc-warrior-berserkers-frenzy", "Berserker's Frenzy", "flame", SkillCategory.Combat, "Gain +40% Damage for 2 turns, but cannot use Defense during this time."),
            ("orc-warrior-executioner", "Executioner", "crossed-swords", SkillCategory.Combat, "Gain +30% Damage against enemies below 25% HP."),
            ("orc-warrior-pain-is-power", "Pain Is Power", "claw", SkillCategory.Combat, "Each 10% HP lost grants +3% Attack."));

        AddBuild(Orc, Bard,
            ("orc-bard-war-drum", "War Drum", "staff", SkillCategory.Utility, "All allies gain +15% Attack."),
            ("orc-bard-blood-song", "Blood Song", "staff", SkillCategory.Utility, "Allies below 50% HP gain +20% Damage."),
            ("orc-bard-fury-chorus", "Fury Chorus", "staff", SkillCategory.Utility, "When an ally kills an enemy, all allies gain +5% Damage for 2 turns."),
            ("orc-bard-savage-rhythm", "Savage Rhythm", "staff", SkillCategory.Utility, "Each consecutive attack by an ally increases Damage by 5%."),
            ("orc-bard-last-song", "Last Song", "crown", SkillCategory.Utility, "If the Bard dies, all allies gain +25% Damage for the next 2 turns."),
            ("orc-bard-death-march", "Death March", "claw", SkillCategory.Utility, "+20% Movement and -10% Defense for the entire team."),
            ("orc-bard-intimidating-roar", "Intimidating Roar", "claw", SkillCategory.Utility, "Nearby enemies suffer -15% Attack for 1 turn."));

        AddBuild(Orc, Healer,
            ("orc-healer-blood-heal", "Blood Heal", "ward", SkillCategory.Survival, "Heals an ally for 25% of their HP, but the caster loses 10% of their own HP."),
            ("orc-healer-blood-ritual", "Blood Ritual", "ward", SkillCategory.Survival, "Sacrifices 15% HP to remove all debuffs from an ally."),
            ("orc-healer-pain-transfer", "Pain Transfer", "shield", SkillCategory.Defense, "Transfers 30% of the damage taken by an ally to the Orc."),
            ("orc-healer-pain-amplification", "Pain Amplification", "flame", SkillCategory.Magic, "+5% Spell Damage for every 10% HP lost."),
            ("orc-healer-savage-recovery", "Savage Recovery", "claw", SkillCategory.Survival, "When an ally kills an enemy, the Orc recovers 5% HP."),
            ("orc-healer-spirit-of-battle", "Spirit of Battle", "ward", SkillCategory.Survival, "Heals more effectively the lower the target's HP is."),
            ("orc-healer-warriors-blessing", "Warrior's Blessing", "crossed-swords", SkillCategory.Utility, "The target gains +20% Damage for 2 turns."));

        AddBuild(Dwarf, Magician,
            ("dwarf-mage-heavy-casting", "Heavy Casting", "staff", SkillCategory.Magic, "Spells are 15% more powerful, but the Dwarf cannot move during the same turn."),
            ("dwarf-mage-mana-forge", "Mana Forge", "mana-drop", SkillCategory.Magic, "Regenerates 1 Mana every 3 turns."),
            ("dwarf-mage-rune-of-power", "Rune of Power", "flame", SkillCategory.Magic, "The next spell deals +30% damage."),
            ("dwarf-mage-stone-barrier", "Stone Barrier", "ward", SkillCategory.Defense, "Creates a shield equal to 25% of Max HP."),
            ("dwarf-mage-ancient-rune", "Ancient Rune", "arcane-star", SkillCategory.Magic, "Places a rune on the battlefield that explodes when an enemy steps on it."),
            ("dwarf-mage-war-forge", "War Forge", "shield", SkillCategory.Defense, "An ally gains +25% Armor for 2 turns."),
            ("dwarf-mage-runic-fortress", "Runic Fortress", "ward", SkillCategory.Defense, "+30% Armor and +30% Magic Resistance for 2 turns, but Movement becomes 0."));

        AddBuild(Dwarf, Warrior,
            ("dwarf-warrior-dwarven-constitution", "Dwarven Constitution", "claw", SkillCategory.Survival, "+15% Max HP."),
            ("dwarf-warrior-mountain-stance", "Mountain Stance", "shield", SkillCategory.Defense, "Cannot be pushed or pulled."),
            ("dwarf-warrior-shield-bash", "Shield Bash", "shield", SkillCategory.Combat, "An attack with a chance to inflict Stun."),
            ("dwarf-warrior-iron-wall", "Iron Wall", "shield", SkillCategory.Defense, "Gain +20% Defense if the Dwarf did not move during the turn."),
            ("dwarf-warrior-runic-armor", "Runic Armor", "ward", SkillCategory.Defense, "+20% Magic Resistance."),
            ("dwarf-warrior-fortress", "Fortress", "shield", SkillCategory.Defense, "Reduces the next incoming attack by 40%."),
            ("dwarf-warrior-unbreakable", "Unbreakable", "crown", SkillCategory.Survival, "The first time it reaches 0 HP, it remains at 1 HP."));

        AddBuild(Dwarf, Bard,
            ("dwarf-bard-anvil-beat", "Anvil Beat", "staff", SkillCategory.Utility, "Nearby enemies suffer -10% Movement."),
            ("dwarf-bard-hammer-rhythm", "Hammer Rhythm", "shield", SkillCategory.Defense, "Allies gain +10% Armor."),
            ("dwarf-bard-forge-song", "Forge Song", "shield", SkillCategory.Defense, "Repairs 15% of an ally's Armor."),
            ("dwarf-bard-drums-of-the-mountain", "Drums of the Mountain", "staff", SkillCategory.Utility, "Allies cannot be pushed or slowed for 1 turn."),
            ("dwarf-bard-iron-chorus", "Iron Chorus", "staff", SkillCategory.Utility, "Each ally gains +5% Damage and +5% Armor."),
            ("dwarf-bard-unyielding-melody", "Unyielding Melody", "ward", SkillCategory.Defense, "When an ally takes fatal damage, they remain at 1 HP, but the Bard loses 20% HP."),
            ("dwarf-bard-living-fortress", "Living Fortress", "shield", SkillCategory.Defense, "The Dwarf cannot heal during this turn, but all nearby allies gain +25% Armor for 2 turns."));

        AddBuild(Dwarf, Healer,
            ("dwarf-healer-stone-heal", "Stone Heal", "ward", SkillCategory.Survival, "Heals 20% HP and grants +10% Armor."),
            ("dwarf-healer-healing-ground", "Healing Ground", "ward", SkillCategory.Survival, "Creates an area that heals 5% HP/turn."),
            ("dwarf-healer-mountain-blessing", "Mountain Blessing", "shield", SkillCategory.Defense, "The ally cannot be Stunned for 2 turns."),
            ("dwarf-healer-rune-of-protection", "Rune of Protection", "ward", SkillCategory.Defense, "The target receives a shield."),
            ("dwarf-healer-ancient-protection", "Ancient Protection", "shield", SkillCategory.Defense, "Reduces the next incoming hit by 30%."),
            ("dwarf-healer-purifying-rune", "Purifying Rune", "gear", SkillCategory.Survival, "Removes Poison, Bleed, and Curse."),
            ("dwarf-healer-stone-skin", "Stone Skin", "shield", SkillCategory.Defense, "+20% Armor."));

        AddBuild(Elf, Magician,
            ("elf-mage-arcane-precision", "Arcane Precision", "arcane-star", SkillCategory.Magic, "+15% Spell Accuracy."),
            ("elf-mage-blink", "Blink", "mirror", SkillCategory.Utility, "Teleport a short distance."),
            ("elf-mage-fey-focus", "Fey Focus", "mana-drop", SkillCategory.Magic, "If the unit has not moved, the next spell costs 1 less Mana."),
            ("elf-mage-fey-illusion", "Fey Illusion", "mirror", SkillCategory.Magic, "Create a clone that draws enemy attacks."),
            ("elf-mage-mana-bloom", "Mana Bloom", "mana-drop", SkillCategory.Magic, "Recover 1 Mana after a critical spell hit."),
            ("elf-mage-moonlight-spell", "Moonlight Spell", "staff", SkillCategory.Magic, "The next spell has +30% Range."),
            ("elf-mage-arcane-perfection", "Arcane Perfection", "crown", SkillCategory.Magic, "The next spell deals +50% Damage, but the Elf cannot use magic on the following turn."));

        AddBuild(Elf, Warrior,
            ("elf-warrior-elven-grace", "Elven Grace", "claw", SkillCategory.Defense, "+15% Dodge."),
            ("elf-warrior-swift-blade", "Swift Blade", "crossed-swords", SkillCategory.Combat, "+15% Attack Speed."),
            ("elf-warrior-dance-of-blades", "Dance of Blades", "claw", SkillCategory.Defense, "+30% Dodge for 1 turn."),
            ("elf-warrior-glass-blade", "Glass Blade", "crossed-swords", SkillCategory.Combat, "+25% Damage, but -15% Armor."),
            ("elf-warrior-perfect-counter", "Perfect Counter", "crossed-swords", SkillCategory.Combat, "If an attack is successfully dodged, can counterattack for 75% Damage."),
            ("elf-warrior-whirling-blades", "Whirling Blades", "crossed-swords", SkillCategory.Combat, "Attacks all adjacent enemies."),
            ("elf-warrior-graceful-step", "Graceful Step", "claw", SkillCategory.Combat, "After dodging an attack, gains +20% Damage for the next attack."));

        AddBuild(Elf, Bard,
            ("elf-bard-charming-tune", "Charming Tune", "staff", SkillCategory.Utility, "Can prevent an enemy from attacking for 1 turn."),
            ("elf-bard-fey-melody", "Fey Melody", "staff", SkillCategory.Utility, "Allies gain +10% Dodge."),
            ("elf-bard-song-of-speed", "Song of Speed", "claw", SkillCategory.Utility, "+20% Movement for the entire team."),
            ("elf-bard-moonlight-ballad", "Moonlit Ballad", "ward", SkillCategory.Survival, "Regenerates 5% HP/turn for 3 turns."),
            ("elf-bard-fey-illusion", "Fey Illusion", "mirror", SkillCategory.Utility, "Creates an illusory copy of an ally for 1 turn."),
            ("elf-bard-elven-harmony", "Elven Harmony", "staff", SkillCategory.Utility, "When two different buffs are active on the same ally, they gain +10% effectiveness."),
            ("elf-bard-eternal-performance", "Eternal Performance", "crown", SkillCategory.Utility, "The last buff used does not expire at the end of the turn, but the Bard cannot attack during that turn."));

        AddBuild(Elf, Healer,
            ("elf-healer-healing-light", "Healing Light", "ward", SkillCategory.Survival, "Heals 25% HP."),
            ("elf-healer-life-bloom", "Life Bloom", "ward", SkillCategory.Survival, "If the target is below 25% HP, the heal is doubled."),
            ("elf-healer-moon-heal", "Moon Heal", "ward", SkillCategory.Survival, "Healing is 30% stronger at night."),
            ("elf-healer-natures-touch", "Nature's Touch", "ward", SkillCategory.Survival, "Heals 10% HP over 3 turns."),
            ("elf-healer-purifying-leaves", "Purifying Leaves", "gear", SkillCategory.Survival, "Removes Poison and Bleeding."),
            ("elf-healer-rebirth-of-nature", "Rebirth of Nature", "crown", SkillCategory.Survival, "Revives an ally with 20% HP, but the Elf loses 30% of their own HP."),
            ("elf-healer-fey-protection", "Fey Protection", "claw", SkillCategory.Defense, "+20% Dodge for the healed target."));

        store.SkillDefinitions.AddRange(skills);
    }
}
