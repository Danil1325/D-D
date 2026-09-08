using DnDGame.Domain.Entities.Classes;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds the 4 classes and their class features (transcribed from the reference
/// cards). All features are seeded at their card-listed level; PrimaryAttribute maps
/// each class onto one of the 5 simplified attributes (Healer and Bard both land on
/// Charisma — there's no rule against two classes sharing a primary attribute).
/// </summary>
internal static class ClassSeedData
{
    public static void Seed(InMemoryGameDataStore store)
    {
        var healer = new CharacterClass
        {
            Id = 1,
            Name = "Healer",
            Description = "Divine casters who restore vitality, mend wounds, and protect allies through the power of light and faith.",
            Quote = "Where there is light, there is hope.",
            CardNumber = 1,
            PrimaryAttribute = AttributeType.Charisma,
            BaseDamageAmount = 3,
            ImageFrontPath = "classes/healer_front.png",
            ImageBackPath = "classes/healer_back.png",
            ArmorDescription = "Light armor, shields.",
            WeaponDescription = "Simple weapons.",
            ToolDescription = "Herbalism kit."
        };

        var warrior = new CharacterClass
        {
            Id = 2,
            Name = "Warrior",
            Description = "Frontline fighters who excel in combat, protect allies, and hold the line against any foe.",
            Quote = "Strength is forged in battle. Courage is proven in every stand.",
            CardNumber = 2,
            PrimaryAttribute = AttributeType.Strength,
            BaseDamageAmount = 6,
            ImageFrontPath = "classes/warrior_front.png",
            ImageBackPath = "classes/warrior_back.png",
            ArmorDescription = "All armor, shields.",
            WeaponDescription = "Simple weapons, martial weapons.",
            ToolDescription = "None."
        };

        var magician = new CharacterClass
        {
            Id = 3,
            Name = "Magician",
            Description = "Masters of the arcane arts, wielding knowledge and illusion to control the battlefield and bend fate to their will.",
            Quote = "Reality is merely the canvas, and magic is the brush.",
            CardNumber = 3,
            PrimaryAttribute = AttributeType.Intelligence,
            BaseDamageAmount = 5,
            ImageFrontPath = "classes/magician_front.png",
            ImageBackPath = "classes/magician_back.png",
            ArmorDescription = "None.",
            WeaponDescription = "Daggers, darts, slings, quarterstaffs, light crossbows.",
            ToolDescription = "Disguise kit, thieves' tools."
        };

        var bard = new CharacterClass
        {
            Id = 4,
            Name = "Bard",
            Description = "Inspirational storytellers, using music, words, and performance to empower allies and sway hearts.",
            Quote = "Through song and tales, we shape the world.",
            CardNumber = 4,
            PrimaryAttribute = AttributeType.Charisma,
            BaseDamageAmount = 4,
            ImageFrontPath = "classes/bard_front.png",
            ImageBackPath = "classes/bard_back.png",
            ArmorDescription = "Light armor.",
            WeaponDescription = "Simple weapons, hand crossbows, longswords, rapiers, shortswords.",
            ToolDescription = "Disguise kit, one musical instrument."
        };

        store.Classes.AddRange(new[] { healer, warrior, magician, bard });

        store.ClassFeatures.AddRange(new[]
        {
            new ClassFeature { Id = 1, ClassId = healer.Id, Name = "Spellcasting", Description = "Cast divine spells drawn from your available spell list.", LevelAcquired = 1 },
            new ClassFeature { Id = 2, ClassId = healer.Id, Name = "Channel Restoration", Description = "Once per encounter, restore health to yourself or an ally.", LevelAcquired = 1 },
            new ClassFeature { Id = 3, ClassId = healer.Id, Name = "Divine Focus", Description = "Use a holy symbol as a focus for your spells.", LevelAcquired = 1 },
            new ClassFeature { Id = 4, ClassId = healer.Id, Name = "Blessed Healer", Description = "Your healing spells restore a little extra health.", LevelAcquired = 4 },
            new ClassFeature { Id = 5, ClassId = healer.Id, Name = "Protective Aura", Description = "Grant nearby allies a small defensive boost.", LevelAcquired = 8 },
            new ClassFeature { Id = 6, ClassId = healer.Id, Name = "Revive", Description = "Once per adventure, restore a fallen ally to a small amount of health.", LevelAcquired = 12 },

            new ClassFeature { Id = 7, ClassId = warrior.Id, Name = "Fighting Style", Description = "Choose a combat specialization that enhances your fighting technique.", LevelAcquired = 1 },
            new ClassFeature { Id = 8, ClassId = warrior.Id, Name = "Second Wind", Description = "Once per encounter, recover a small amount of health as a bonus action.", LevelAcquired = 1 },
            new ClassFeature { Id = 9, ClassId = warrior.Id, Name = "Action Surge", Description = "Take an additional action once per encounter.", LevelAcquired = 4 },
            new ClassFeature { Id = 10, ClassId = warrior.Id, Name = "Martial Archetype", Description = "Specialize into a particular martial tradition.", LevelAcquired = 4 },
            new ClassFeature { Id = 11, ClassId = warrior.Id, Name = "Extra Attack", Description = "Attack twice instead of once when you take the Attack action.", LevelAcquired = 8 },
            new ClassFeature { Id = 12, ClassId = warrior.Id, Name = "Indomitable", Description = "Once per adventure, reroll a failed check.", LevelAcquired = 12 },

            new ClassFeature { Id = 13, ClassId = magician.Id, Name = "Spellcasting", Description = "Cast arcane spells drawn from your spellbook.", LevelAcquired = 1 },
            new ClassFeature { Id = 14, ClassId = magician.Id, Name = "Arcane Recovery", Description = "Once per adventure, recover some of your spent spellcasting resources.", LevelAcquired = 1 },
            new ClassFeature { Id = 15, ClassId = magician.Id, Name = "Spellbook", Description = "Record and study spells in a spellbook you carry.", LevelAcquired = 1 },
            new ClassFeature { Id = 16, ClassId = magician.Id, Name = "Arcane Tradition", Description = "Specialize into a particular school of magic.", LevelAcquired = 4 },
            new ClassFeature { Id = 17, ClassId = magician.Id, Name = "Sleight of Hand", Description = "Perform small feats of dexterity using minor magic.", LevelAcquired = 4 },
            new ClassFeature { Id = 18, ClassId = magician.Id, Name = "Mirror Image", Description = "Create illusory duplicates of yourself to confuse attackers.", LevelAcquired = 8 },
            new ClassFeature { Id = 19, ClassId = magician.Id, Name = "Arcane Ward", Description = "Surround yourself with a shield of magical energy.", LevelAcquired = 12 },

            new ClassFeature { Id = 20, ClassId = bard.Id, Name = "Bardic Inspiration", Description = "Inspire an ally, granting them a bonus to an upcoming check.", LevelAcquired = 1 },
            new ClassFeature { Id = 21, ClassId = bard.Id, Name = "Spellcasting", Description = "Cast a small number of spells through music and performance.", LevelAcquired = 1 },
            new ClassFeature { Id = 22, ClassId = bard.Id, Name = "Jack of All Trades", Description = "Add a small bonus to checks you'd otherwise have no training in.", LevelAcquired = 4 },
            new ClassFeature { Id = 23, ClassId = bard.Id, Name = "Song of Rest", Description = "Help allies recover a little extra health during a short rest.", LevelAcquired = 4 },
            new ClassFeature { Id = 24, ClassId = bard.Id, Name = "Expertise", Description = "Become exceptionally skilled in a chosen area.", LevelAcquired = 8 },
            new ClassFeature { Id = 25, ClassId = bard.Id, Name = "Countercharm", Description = "Use music to help allies resist certain magical effects.", LevelAcquired = 8 },
            new ClassFeature { Id = 26, ClassId = bard.Id, Name = "Magical Secrets", Description = "Learn a spell from outside your usual repertoire.", LevelAcquired = 12 }
        });
    }
}
