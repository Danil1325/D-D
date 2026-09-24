using DnDGame.Domain.Entities.Shop;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds the Shop item catalog, transcribed from the frontend's
/// src/assets/items asset manifest (src/assets/items/index.ts builds
/// cardImagePairs from every complete front/back PNG pair under
/// src/assets/items/{weapons,armor,potions,artifacts}/, and
/// src/data/marketItems.ts turns those into buyable/sellable MarketItems).
/// Code and Name are derived the same way the frontend derives them from each
/// asset's folder/file slug, so both sides stay keyed the same way (a couple of
/// source filenames carry typos, e.g. "clairevoyance", "masck" — kept verbatim
/// rather than silently "corrected").
///
/// Pricing uses the frontend's own formula (BuyPrice = category base + (index
/// mod 5) * 25, SellPrice = BuyPrice / 2) but keyed to each item's position
/// within its own category here, not the frontend's single alphabetically
/// sorted list across all categories — the frontend's index-derived price was
/// itself just a placeholder generator, not deliberate game balance, so exact
/// per-item parity isn't meaningful; only the formula and category base prices
/// are carried over.
/// </summary>
internal static class ShopSeedData
{
    private const int WeaponsBasePrice = 250;
    private const int ArmorBasePrice = 350;
    private const int PotionsBasePrice = 50;
    private const int ArtifactsBasePrice = 400;

    public static void Seed(InMemoryGameDataStore store)
    {
        var nextId = 1;
        var items = new List<ShopItem>();

        void AddCategory(ShopItemCategory category, int basePrice, params (string Code, string Name)[] rows)
        {
            for (var index = 0; index < rows.Length; index++)
            {
                var buyPrice = basePrice + (index % 5) * 25;
                items.Add(new ShopItem
                {
                    Id = nextId++,
                    Code = rows[index].Code,
                    Name = rows[index].Name,
                    Category = category,
                    BuyPrice = buyPrice,
                    SellPrice = buyPrice / 2
                });
            }
        }

        AddCategory(ShopItemCategory.Weapons, WeaponsBasePrice,
            ("weapons-the-arcane-staff", "The Arcane Staff"),
            ("weapons-the-war-axe", "The War Axe"),
            ("weapons-the-elven-bow", "The Elven Bow"),
            ("weapons-the-shadow-dagger", "The Shadow Dagger"),
            ("weapons-the-sword", "The Sword"));

        AddCategory(ShopItemCategory.Armor, ArmorBasePrice,
            ("armor-the-cloak-of-eclipse", "The Cloak Of Eclipse"));

        AddCategory(ShopItemCategory.Potions, PotionsBasePrice,
            ("potions-potion-of-acid-resistance", "Potion Of Acid Resistance"),
            ("potions-potion-of-animal-friendship", "Potion Of Animal Friendship"),
            ("potions-potion-of-clairevoyance", "Potion Of Clairevoyance"),
            ("potions-potion-of-climbing", "Potion Of Climbing"),
            ("potions-potion-of-cloud-giant-strength", "Potion Of Cloud Giant Strength"),
            ("potions-potion-of-cold-resistance", "Potion Of Cold Resistance"),
            ("potions-potion-of-diminution", "Potion Of Diminution"),
            ("potions-potion-of-fire-giant-strength", "Potion Of Fire Giant Strength"),
            ("potions-potion-of-fire-resistance", "Potion Of Fire Resistance"),
            ("potions-potion-of-flying", "Potion Of Flying"),
            ("potions-potion-of-frost-giant-strength", "Potion Of Frost Giant Strength"),
            ("potions-potion-of-gaseous-form", "Potion Of Gaseous Form"),
            ("potions-potion-of-greater-healing", "Potion Of Greater Healing"),
            ("potions-potion-of-growth", "Potion Of Growth"),
            ("potions-potion-healing", "Potion Healing"),
            ("potions-potion-of-heroism", "Potion Of Heroism"),
            ("potions-potion-of-hill-giant-strength", "Potion Of Hill Giant Strength"),
            ("potions-potion-of-invisibility", "Potion Of Invisibility"),
            ("potions-potion-of-invulnerability", "Potion Of Invulnerability"),
            ("potions-potion-of-lightning-resistance", "Potion Of Lightning Resistance"),
            ("potions-potion-of-longevity", "Potion Of Longevity"),
            ("potions-potion-of-mana", "Potion Of Mana"),
            ("potions-potion-of-mind-reading", "Potion Of Mind Reading"),
            ("potions-potion-of-necrotic-resistance", "Potion Of Necrotic Resistance"),
            ("potions-potion-of-poison-resistance", "Potion Of Poison Resistance"),
            ("potions-potion-of-speed", "Potion Of Speed"),
            ("potions-potion-of-superior-healing", "Potion Of Superior Healing"),
            ("potions-potion-of-supreme-healing", "Potion Of Supreme Healing"),
            ("potions-potion-of-vitality", "Potion Of Vitality"),
            ("potions-potion-of-water-breathing", "Potion Of Water Breathing"));

        AddCategory(ShopItemCategory.Artifacts, ArtifactsBasePrice,
            ("artifacts-the-ancient-grimoire", "The Ancient Grimoire"),
            ("artifacts-the-bell-of-the-dead", "The Bell Of The Dead"),
            ("artifacts-the-black-lotus", "The Black Lotus"),
            ("artifacts-the-serpent-bracelet", "The Serpent Bracelet"),
            ("artifacts-the-chains-of-the-abyss", "The Chains Of The Abyss"),
            ("artifacts-the-chalice-of-eternity", "The Chalice Of Eternity"),
            ("artifacts-the-celestial-compass", "The Celestial Compass"),
            ("artifacts-the-crown-of-ashes", "The Crown Of Ashes"),
            ("artifacts-the-crystal-of-souls", "The Crystal Of Souls"),
            ("artifacts-the-dice-of-eate", "The Dice Of Eate"),
            ("artifacts-the-eye-of-the-void", "The Eye Of The Void"),
            ("artifacts-the-heart-of-the-mountain", "The Heart Of The Mountain"),
            ("artifacts-the-horn-of-the-ancient", "The Horn Of The Ancient"),
            ("artifacts-the-hourglass-of-time", "The Hourglass Of Time"),
            ("artifacts-the-key-of-realms", "The Key Of Realms"),
            ("artifacts-the-lantern-of-lost-souls", "The Lantern Of Lost Souls"),
            ("artifacts-the-masck-of-shadows", "The Masck Of Shadows"),
            ("artifacts-the-mirror-of-echoes", "The Mirror Of Echoes"),
            ("artifacts-the-moon-amulet", "The Moon Amulet"),
            ("artifacts-the-necromancers-skull", "The Necromancers Skull"),
            ("artifacts-the-orb-of-vision", "The Orb Of Vision"),
            ("artifacts-the-phoenix-feather", "The Phoenix Feather"),
            ("artifacts-the-dragon-ring", "The Dragon Ring"),
            ("artifacts-the-seal-of-the-forgotten-king", "The Seal Of The Forgotten King"));

        store.ShopItems.AddRange(items);
    }
}
