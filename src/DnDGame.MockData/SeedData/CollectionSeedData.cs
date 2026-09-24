using DnDGame.Domain.Entities.Collection;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds the Collection catalog, transcribed from the frontend's
/// src/pages/Collection/Collection.tsx collectionData array. Code values match
/// the frontend's item ids exactly (e.g. "iron-sword") so both sides stay keyed
/// the same way. No "discovered" state is modeled — see CollectionEntry's remarks.
/// </summary>
internal static class CollectionSeedData
{
    public static void Seed(InMemoryGameDataStore store)
    {
        var nextId = 1;
        var entries = new List<CollectionEntry>();

        void Add(
            string code,
            CollectionCategory category,
            string name,
            string description,
            CollectionRarity rarity,
            IReadOnlyDictionary<string, string>? stats = null,
            string? lore = null)
        {
            entries.Add(new CollectionEntry
            {
                Id = nextId++,
                Code = code,
                Category = category,
                Name = name,
                Description = description,
                Rarity = rarity,
                Stats = stats,
                Lore = lore
            });
        }

        // Heroes
        Add("orc", CollectionCategory.Heroes, "The Orc", "A fierce warrior from the outlands.", CollectionRarity.Legendary,
            new Dictionary<string, string> { ["HP"] = "200", ["Mana"] = "50" },
            "Driven from his homeland, he seeks redemption and the Crown of Ash.");

        // Classes
        Add("warrior", CollectionCategory.Classes, "Warrior", "Masters of melee combat and physical defense.", CollectionRarity.Common,
            lore: "Warriors focus on brute strength and endurance, thriving in the frontline of any battle.");
        Add("mage", CollectionCategory.Classes, "Mage", "Wielders of arcane magic and elemental forces.", CollectionRarity.Common,
            lore: "Mages trade physical resilience for devastating area attacks and crowd control.");
        Add("paladin", CollectionCategory.Classes, "Paladin", "Holy knights who mix combat with healing arts.", CollectionRarity.Common,
            lore: "Sworn to divine oaths, paladins protect their allies and smite the wicked.");

        // Enemies - Level I
        Add("skeleton", CollectionCategory.Enemies, "Skeleton", "Reanimated bones bound by dark magic.", CollectionRarity.Common,
            new Dictionary<string, string> { ["HP"] = "20", ["Damage"] = "3" },
            "Usually found in ancient ruins, guarding their final resting place.");
        Add("goblin", CollectionCategory.Enemies, "Goblin", "Sneaky and fast enemies found in the lowlands.", CollectionRarity.Common,
            new Dictionary<string, string> { ["HP"] = "30", ["Damage"] = "5" },
            "Goblins hunt in packs and are known for setting nasty traps.");
        Add("troll", CollectionCategory.Enemies, "Troll", "A hulking beast with regenerative abilities.", CollectionRarity.Uncommon);
        Add("slime", CollectionCategory.Enemies, "Slime", "Acidic gelatinous cube.", CollectionRarity.Common);
        Add("phantom", CollectionCategory.Enemies, "Phantom", "An ethereal spirit that haunts the living.", CollectionRarity.Uncommon);
        Add("chimera", CollectionCategory.Enemies, "Chimera", "A terrifying amalgamation of beasts.", CollectionRarity.Rare);
        Add("demon", CollectionCategory.Enemies, "Demon", "A spawn from the underworld.", CollectionRarity.Rare);

        // Enemies - Level II
        Add("skeleton-knight", CollectionCategory.Enemies, "Skeleton Knight", "A skeleton clad in rusty armor.", CollectionRarity.Uncommon);
        Add("hobgoblin", CollectionCategory.Enemies, "Hobgoblin", "A larger, more aggressive goblin variant.", CollectionRarity.Uncommon);
        Add("warrior-troll", CollectionCategory.Enemies, "Warrior Troll", "A troll armed with crude weapons.", CollectionRarity.Rare);
        Add("great-slime", CollectionCategory.Enemies, "Great Slime", "A massive slime capable of engulfing adventurers.", CollectionRarity.Uncommon);
        Add("wraith", CollectionCategory.Enemies, "Wraith", "A vengeful spirit with a chilling touch.", CollectionRarity.Rare);
        Add("great-chimera", CollectionCategory.Enemies, "Great Chimera", "An older, more dangerous chimera.", CollectionRarity.Epic);
        Add("greater-demon", CollectionCategory.Enemies, "Greater Demon", "A high-ranking demon of immense power.", CollectionRarity.Epic);

        // Enemies - Level III
        Add("the-lich", CollectionCategory.Enemies, "The Lich", "An undead sorcerer of unimaginable power.", CollectionRarity.Legendary);
        Add("lordgoblin", CollectionCategory.Enemies, "Lordgoblin", "The absolute ruler of the goblin hordes.", CollectionRarity.Epic);
        Add("troll-king", CollectionCategory.Enemies, "Troll King", "The ancient king of all trolls.", CollectionRarity.Legendary);
        Add("king-slime", CollectionCategory.Enemies, "King Slime", "The original slime from which all others split.", CollectionRarity.Legendary);
        Add("dread-wraith", CollectionCategory.Enemies, "Dread Wraith", "A wraith that feeds on sheer terror.", CollectionRarity.Epic);
        Add("divine-chimera", CollectionCategory.Enemies, "Divine Chimera", "A mythical chimera with celestial traits.", CollectionRarity.Legendary);
        Add("demon-lord", CollectionCategory.Enemies, "Demon Lord", "The ruler of the underworld.", CollectionRarity.Legendary);

        // Weapons
        Add("iron-sword", CollectionCategory.Weapons, "Iron Sword", "Reliable and sharp.", CollectionRarity.Common,
            new Dictionary<string, string> { ["Damage"] = "12" });
        Add("elven-bow", CollectionCategory.Weapons, "Elven Bow", "Swift and silent.", CollectionRarity.Rare,
            new Dictionary<string, string> { ["Damage"] = "18", ["Speed"] = "+10%" });
        Add("dragon-blade", CollectionCategory.Weapons, "Dragon Blade", "Forged in fire.", CollectionRarity.Epic,
            new Dictionary<string, string> { ["Damage"] = "35", ["Fire"] = "+15" });
        Add("mage-staff", CollectionCategory.Weapons, "Mage Staff", "Channel the arcane.", CollectionRarity.Uncommon,
            new Dictionary<string, string> { ["Damage"] = "10", ["Magic"] = "+20" });

        // Armor
        Add("heavy-armor", CollectionCategory.Armor, "Heavy Armor", "Stand firm.", CollectionRarity.Rare,
            new Dictionary<string, string> { ["Defense"] = "25", ["Speed"] = "-5%" });
        Add("leather-armor", CollectionCategory.Armor, "Leather Armor", "Light and flexible.", CollectionRarity.Common,
            new Dictionary<string, string> { ["Defense"] = "10" });

        // Potions
        Add("healing-potion", CollectionCategory.Potions, "Healing Potion", "Restores your health.", CollectionRarity.Common,
            new Dictionary<string, string> { ["Effect"] = "Restore 50 HP" });
        Add("mana-potion", CollectionCategory.Potions, "Mana Potion", "Replenishes your mana.", CollectionRarity.Common,
            new Dictionary<string, string> { ["Effect"] = "Restore 30 Mana" });

        store.CollectionEntries.AddRange(entries);
    }
}
