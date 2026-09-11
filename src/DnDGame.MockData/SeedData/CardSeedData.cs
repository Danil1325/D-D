using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>Catalogue cards supplied by the game design. No repository or database is involved.</summary>
internal static class CardSeedData
{
    public static void Seed(InMemoryGameDataStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        if (store.Cards.Count != 0) return;

        store.Cards.AddRange(
        [
            Card(1, "The Elven Bow", CardType.Weapon, CardRarity.Epic, "Ranged / Two-Handed", 24, "Piercing", range: "Long", active: "Eagle's Mark", passive: "+30% Critical Chance at Long Range", bonus: "+5 Agility", penalty: "-15% Accuracy at Close Range", lore: "Its arrows follow the moonlight through even the darkest forest."),
            Card(2, "The Sword", CardType.Weapon, CardRarity.Rare, "Melee / One-Handed", 22, "Physical", weight: "Medium", active: "Rally", passive: "+15% Damage near an Ally", bonus: "+4 Strength", penalty: "-10% Defense while Alone", lore: "Forged to unite rival kingdoms, it grows stronger beside a loyal ally."),
            Card(3, "The Arcane Staff", CardType.Weapon, CardRarity.Epic, "Magic / Two-Handed", 26, "Arcane", cost: 18, active: "Arcane Surge", passive: "Restores 10 Mana after a Critical Hit", bonus: "+6 Intelligence", penalty: "-12% Physical Defense", lore: "Grown around a fallen star, it whispers spells forgotten by mortals."),
            Card(4, "The Shadow Dagger", CardType.Weapon, CardRarity.Rare, "Melee / One-Handed", 18, "Physical", weight: "Fast", active: "Shadowstep", passive: "First Stealth Strike deals +50% Damage", bonus: "+6 Agility", penalty: "-20% Damage in Direct Combat", lore: "Bound to its wielder's shadow, it drinks the light before every strike."),
            Artifact(5, "The Moon Amulet", CardRarity.Epic, "Magical / Amulet", defense: 14, defenseType: "Magic", charges: "1 per Battle", active: "Lunar Veil", passive: "Prevents one Fatal Hit", bonus: "+5 Wisdom", penalty: "-10% Physical Damage", lore: "Born from a shard of the first moon, it guards those who walk between dreams."),
            Artifact(6, "The Dragon Ring", CardRarity.Legendary, "Magical / Ring", power: 20, powerType: "Fire", charges: "2 per Battle", active: "Dragon's Fury", passive: "+25% Damage below 30% Health", bonus: "+5 Strength", penalty: "-10% Ice Resistance", lore: "Forged in the last dragon's breath, it awakens when its bearer refuses to fall."),
            Artifact(7, "The Orb of Vision", CardRarity.Legendary, "Divination / Orb", power: 18, powerType: "Arcane", charges: "2 per Battle", active: "Foresight", passive: "Reveals the Enemy's Next Action", bonus: "+6 Perception", penalty: "-15% Defense while Active", lore: "It does not show the future—it reveals the choice you fear most."),
            Artifact(8, "The Ancient Grimoire", CardRarity.Legendary, "Arcane / Grimoire", cost: -20, charges: "3 Stored Spells", active: "Forbidden Knowledge", passive: "Recasts one Spell without Mana", bonus: "+7 Intelligence", penalty: "-8 Health after Recasting", lore: "Every page remembers a mage whose name the world was made to forget."),
            Artifact(9, "The Crown of Ashes", CardRarity.Legendary, "Cursed / Crown", defense: 12, defenseType: "Fire", charges: "1 per Battle", active: "Ashen Rebirth", passive: "Revives with 35% Health", bonus: "+6 Willpower", penalty: "-15% Maximum Health", lore: "It crowns no living ruler—only those who have already burned."),
            Artifact(10, "The Phoenix Feather", CardRarity.Legendary, "Magical / Relic", power: 18, powerType: "Fire", charges: "Single Use", active: "Flame of Rebirth", passive: "Revives one Ally with 40% Health", bonus: "+5 Spirit", penalty: "Consumed after Activation", lore: "When its final ember fades, a fallen soul rises with the dawn."),
            Artifact(11, "The Eye of the Void", CardRarity.Legendary, "Cursed / Relic", power: 24, powerType: "Void", charges: "2 per Battle", active: "Abyssal Gaze", passive: "Silences one Enemy for 2 Turns", bonus: "+6 Perception", penalty: "-10 Health per Activation", lore: "It reveals every hidden truth, and something beyond always looks back."),
            Artifact(12, "The Hourglass of Time", CardRarity.Legendary, "Temporal / Relic", charges: "1 per Battle", activation: "Instant", active: "Time Reversal", passive: "Rewinds the Previous Turn", bonus: "+8 Initiative", penalty: "Skips the Next Turn", lore: "Its sand flows upward whenever fate realizes it has made a mistake."),
            Artifact(13, "The Necromancer's Skull", CardRarity.Legendary, "Necromancy / Focus", power: 22, powerType: "Necrotic", charges: "2 per Battle", active: "Soul Command", passive: "Summons one Fallen Enemy for 2 Turns", bonus: "+6 Intelligence", penalty: "-12 Health per Summon", lore: "It speaks in the voices of the dead and remembers every debt left unpaid."),
            Artifact(14, "The Celestial Compass", CardRarity.Epic, "Celestial / Compass", charges: "3 per Quest", range: "Global", active: "True Bearing", passive: "Reveals the Safest Route", bonus: "+5 Perception", penalty: "Fails near Void Magic", lore: "It never points north—only toward the destination its bearer needs most."),
            Artifact(15, "The Crystal of Souls", CardRarity.Legendary, "Necromancy / Crystal", power: 20, powerType: "Necrotic", charges: "Capacity: 3 Souls", active: "Soul Harvest", passive: "Absorbs a Fallen Enemy for 15 Mana", bonus: "+6 Spell Power", penalty: "-5 Health per Stored Soul", lore: "Each facet holds a final breath, waiting for a voice strong enough to command it."),
            Artifact(16, "The Serpent Bracelet", CardRarity.Epic, "Magical / Bracelet", power: 16, powerType: "Poison", active: "Venom Coil", passive: "Attacks inflict Poison for 3 Turns; +20% Poison Resistance", bonus: "+5 Agility", penalty: "-10% Healing Received", lore: "Its serpent sleeps against the pulse, waking only when it tastes betrayal."),
            Artifact(17, "The Mask of Shadows", CardRarity.Epic, "Stealth / Mask", power: 18, powerType: "Shadow", charges: "2 per Battle", active: "Veil of Shadows", passive: "Grants Invisibility for 2 Turns", bonus: "+6 Agility", penalty: "-15% Defense when Revealed", lore: "No one remembers the face beneath it—not even the one who wears it."),
            Artifact(18, "The Lantern of Lost Souls", CardRarity.Epic, "Spirit / Lantern", power: 18, powerType: "Spirit", charges: "3 per Quest", range: "Medium", active: "Soul Beacon", passive: "Reveals Hidden Spirits and Traps", bonus: "+5 Perception", penalty: "Attracts Undead Enemies", lore: "Its flame is fed by souls that no longer remember the road home."),
            Artifact(19, "The Key of Realms", CardRarity.Legendary, "Portal / Key", charges: "2 per Quest", range: "Global", active: "Realmwalk", passive: "Opens a Portal to a Discovered Location", bonus: "+5 Arcane Mastery", penalty: "Enemies may Enter the Portal", lore: "Every lock knows this key, but each door remembers who crossed it."),
            Artifact(20, "The Mirror of Echoes", CardRarity.Legendary, "Illusion / Mirror", charges: "2 per Battle", range: "Medium", active: "Echo Reflection", passive: "Copies the Enemy's Last Ability", bonus: "+6 Intelligence", penalty: "Copied Ability costs +50% Mana", lore: "It reflects no present face—only the choice that face regrets most."),
            Artifact(21, "The Chalice of Eternity", CardRarity.Legendary, "Divine / Chalice", power: 20, powerType: "Divine", charges: "1 per Quest", active: "Eternal Draught", passive: "Restores all Health and Mana", bonus: "+5 Vitality", penalty: "-20% Maximum Health for the Quest", lore: "It grants another tomorrow—by quietly taking one from the end."),
            Artifact(22, "The Seal of the Forgotten King", CardRarity.Legendary, "Royal / Seal", charges: "2 per Battle", range: "Party", active: "King's Decree", passive: "Allies gain +20% Defense for 2 Turns", bonus: "+6 Charisma", penalty: "User cannot Attack while Active", lore: "The kingdom forgot his name, but every oath still recognizes his seal."),
            Card(23, "The Cloak of Eclipse", CardType.Armor, CardRarity.Legendary, "Light / Cloak", 0, null, defense: 16, defenseType: "Shadow", weight: "Light", active: "Eclipse Shroud", passive: "Negates the First Ranged Attack", bonus: "+6 Evasion", penalty: "-20% Light Resistance", lore: "Woven from the moment day surrendered its final light to night."),
            Artifact(24, "The Bell of the Dead", CardRarity.Legendary, "Necromancy / Bell", power: 20, powerType: "Necrotic", charges: "1 per Battle", range: "All Enemies", active: "Death Toll", passive: "Enemies lose 20% Speed for 2 Turns", bonus: "+5 Willpower", penalty: "Allies lose 10% Speed for 1 Turn", lore: "It rings only once—yet the dead hear its echo forever."),
            Artifact(25, "The Dice of Fate", CardRarity.Legendary, "Chance / Dice", charges: "1 per Battle", activation: "Instant", active: "Twist of Fate", passive: "Rerolls any Failed Action", bonus: "+5 Luck", penalty: "The New Result must be Accepted", lore: "They do not predict fate—they decide which version survives."),
            Artifact(26, "The Horn of the Ancients", CardRarity.Legendary, "War / Horn", power: 18, powerType: "Sonic", charges: "1 per Battle", range: "Party", active: "Ancestral Call", passive: "Allies gain +20% Damage for 2 Turns", bonus: "+6 Strength", penalty: "Reveals the Party's Location", lore: "Its call crosses centuries, and every fallen warrior answers in silence."),
            Consumable(27, "The Black Lotus", CardRarity.Legendary, "Alchemy / Flower", power: 18, powerType: "Poison", charges: "Single Use", range: "Medium", active: "Dreamless Sleep", passive: "Puts one Enemy to Sleep for 2 Turns", bonus: "+5 Alchemy", penalty: "Target Awakens when Damaged", lore: "It blooms where a promise dies, and closes when someone dreams of it."),
            Artifact(28, "The Heart of the Mountain", CardRarity.Legendary, "Earth / Crystal", defense: 24, defenseType: "Earth", charges: "1 per Battle", active: "Stoneheart", passive: "Grants a Shield equal to 30% Max Health", bonus: "+7 Vitality", penalty: "-20% Movement Speed", lore: "It beats once each century, and every mountain answers below."),
            Artifact(29, "The Chains of the Abyss", CardRarity.Legendary, "Void / Chains", power: 22, powerType: "Void", charges: "2 per Battle", range: "Medium", active: "Abyssal Bind", passive: "Restrains one Enemy for 2 Turns", bonus: "+5 Strength", penalty: "User cannot Move while Active", lore: "Forged to imprison a fallen god, each link still remembers its name."),
            Card(30, "The War Axe", CardType.Weapon, CardRarity.Rare, "Melee / Two-Handed", 28, "Physical", weight: "Heavy", active: "Cleave", passive: "Hits up to 2 Nearby Enemies", bonus: "+6 Strength", penalty: "-10% Attack Speed", lore: "A brutal rune-forged axe, feared wherever battle banners rise."),
            Potion(31, "Potion of Superior Healing", CardRarity.Rare, "Healing", 35, "Healing", "Superior Restoration", "Restores 70 Health Instantly", "Removes Bleeding, Poison and Burn", "Cannot Exceed Maximum Health", "A radiant elixir said to pull wounded souls back from the edge of darkness."),
            Potion(32, "Potion of Supreme Healing", CardRarity.VeryRare, "Healing", 50, "Healing", "Supreme Restoration", "Restores 100 Health Instantly", "Removes All Negative Conditions", "Cannot Exceed Maximum Health", "A drop of dawn captured in crystal—powerful enough to call life back to the fallen."),
            Potion(33, "Potion of Vitality", CardRarity.VeryRare, "Vitality", 30, "Vitality", "Vital Surge", "Restores 50 Health and Stamina", "Removes Poison and Exhaustion", "Effect Cannot Stack", "Its living flame rekindles the strength that weariness tried to steal."),
            Potion(34, "Potion of Mana", CardRarity.Uncommon, "Arcane", 25, "Mana", "Arcane Replenishment", "Restores 40 Mana Instantly", "Next Spell Costs 5 Less Mana", "Cannot Exceed Maximum Mana", "Moonlight distilled into liquid, a quiet tide that restores forgotten magic."),
            Potion(35, "Potion of Hill Giant Strength", CardRarity.Uncommon, "Strength", 21, "Strength", "Hill Giant Might", "Sets Strength to 21 for 5 Turns", "+5 Melee Damage", "No Effect if Strength is Already 21+", "Brewed with stone and thunder, it lends mortal hands the might of the hills."),
            Potion(36, "Potion of Frost Giant Strength", CardRarity.Rare, "Strength", 23, "Strength", "Frost Giant Might", "Sets Strength to 23 for 5 Turns", "+7 Melee Damage", "No Effect if Strength is Already 23+", "Cold as a giant's breath, it turns mortal muscle into living glacier."),
            Potion(37, "Potion of Fire Giant Strength", CardRarity.Rare, "Strength", 25, "Strength", "Fire Giant Might", "Sets Strength to 25 for 5 Turns", "+9 Melee Damage", "No Effect if Strength is Already 25+", "Forged in a giant's furnace, every drop carries the weight of molten iron."),
            Potion(38, "Potion of Cloud Giant Strength", CardRarity.VeryRare, "Strength", 27, "Strength", "Cloud Giant Might", "Sets Strength to 27 for 5 Turns", "+11 Melee Damage", "No Effect if Strength is Already 27+", "Born above the highest peaks, it grants the crushing might of the storm."),
            Potion(39, "Potion of Speed", CardRarity.VeryRare, "Enhancement", 10, "Speed", "Lightning Reflexes", "Grants One Extra Action for 3 Turns", "+2 Defense and Evasion", "Exhausted for 1 Turn After Effect Ends", "Bottled lightning races through the veins, turning every heartbeat into a thunderclap."),
            Potion(40, "Potion of Heroism", CardRarity.Rare, "Enhancement", 10, "Temporary Health", "Courage of the Lion", "+2 Attack for 5 Turns", "Immune to Fear for 5 Turns", "Temporary Health Vanishes When Effect Ends", "A lion's courage sealed in sapphire—drink, and no shadow can command your heart."),
            Potion(41, "Potion of Invulnerability", CardRarity.Legendary, "Protection", 50, "Damage Reduction", "Adamantine Ward", "Resists All Damage for 3 Turns", "Immune to Critical Hits", "-3 Defense for 1 Turn After Effect Ends", "A fortress distilled into a single draught; for a moment, even fate cannot break you."),
            Potion(42, "Potion of Growth", CardRarity.Uncommon, "Enhancement", 5, "Strength", "Verdant Expansion", "Become Large for 5 Turns", "+4 Melee Damage and Reach", "-2 Evasion While Enlarged", "Brewed from roots that split ancient stone, one sip awakens the giant sleeping within."),
            Potion(43, "Potion of Fire Resistance", CardRarity.Uncommon, "Protection", 50, "Fire Resistance", "Flame Ward", "Halves Fire Damage for 5 Turns", "Immune to Burning", "-2 Frost Resistance While Active", "Distilled from the breath of a dying flame, fire may touch you, but it cannot claim you."),
            Potion(44, "Potion of Cold Resistance", CardRarity.Uncommon, "Protection", 50, "Cold Resistance", "Winter Ward", "Halves Cold Damage for 5 Turns", "Immune to Frozen", "-2 Fire Resistance While Active", "A captured ember defies the endless winter; cold may surround you, but it cannot enter."),
            Potion(45, "Potion of Acid Resistance", CardRarity.Uncommon, "Protection", 50, "Acid Resistance", "Caustic Ward", "Halves Acid Damage for 5 Turns", "Immune to Corrosion", "-2 Poison Resistance While Active", "Refined where venom eats through iron, acid may hiss against you, but leaves no scar."),
            Potion(46, "Potion of Poison Resistance", CardRarity.Uncommon, "Protection", 50, "Poison Resistance", "Venom Ward", "Halves Poison Damage for 5 Turns", "Immune to Poisoned", "-2 Acid Resistance While Active", "Brewed from venom that once stopped a king; poison may enter, but it cannot remain."),
            Potion(47, "Potion of Necrotic Resistance", CardRarity.Rare, "Protection", 50, "Necrotic Resistance", "Soul Ward", "Halves Necrotic Damage for 5 Turns", "Immune to Maximum Health Reduction", "Receives 25% Less Healing While Active", "A fragment of dawn sealed against the grave—death may reach for you, but finds no hold."),
            Potion(48, "Potion of Invisibility", CardRarity.Rare, "Stealth", 3, "Turns", "Veil of Glass", "Become Invisible for Up to 3 Turns", "+5 Stealth and Advantage on First Attack", "Ends After Attacking or Casting a Spell", "Distilled from the space between reflections, drink, and the world forgets where you stand."),
            Potion(49, "Potion of Flying", CardRarity.Rare, "Mobility", 5, "Turns", "Wings of Aether", "Gain Flight for 5 Turns", "+3 Evasion While Airborne", "Fall if Effect Ends While Airborne", "The open sky folded into a sapphire draught, and the earth releases its claim."),
            Potion(50, "Potion of Gaseous Form", CardRarity.Rare, "Transformation", 3, "Turns", "Mistbound Body", "Become Gaseous for 3 Turns", "50% Physical Resistance", "Cannot Attack or Cast Spells", "A wandering cloud captured beneath glass—drink, and slip through the world without a footprint."),
            Potion(51, "Potion of Water Breathing", CardRarity.Uncommon, "Exploration", 10, "Turns", "Tidal Lungs", "Breathe Underwater for 10 Turns", "+3 Movement Underwater", "No Protection from Water Pressure", "The ocean's breath sleeps within this shell, and every current becomes your air."),
            Potion(52, "Potion of Climbing", CardRarity.Common, "Exploration", 10, "Turns", "Spider's Ascent", "Climb Vertical Surfaces for 10 Turns", "+5 Climbing Checks", "Cannot Climb Smooth or Magical Surfaces", "Root and rope entwined beneath the mountain—every wall becomes a path."),
            Potion(53, "Potion of Diminution", CardRarity.Rare, "Transformation", 5, "Turns", "Lesser Form", "Shrink One Size for 5 Turns", "+4 Evasion and Stealth", "-4 Melee Damage While Reduced", "A giant's shadow folded into a violet drop, and the world grows vast around you."),
            Potion(54, "Potion of Animal Friendship", CardRarity.Uncommon, "Enchantment", 10, "Turns", "Wildheart Bond", "Charm One Beast for 10 Turns", "+5 Animal Handling", "Effect Ends if the Beast is Harmed", "Brewed where pawprints cross beneath green leaves, and the wild remembers you as a friend."),
            Potion(55, "Potion of Mind Reading", CardRarity.Rare, "Divination", 3, "Turns", "Whispered Insight", "Read Surface Thoughts for 3 Turns", "+5 Insight and Deception", "No Effect on Mindless or Shielded Targets", "Starlight gathered behind an unblinking eye, and hidden thoughts begin to whisper."),
            Potion(56, "Potion of Clairvoyance", CardRarity.Rare, "Divination", 5, "Turns", "Distant Sight", "Observe an Unseen Place for 5 Turns", "+5 Perception While Scrying", "Unaware of Nearby Surroundings", "A distant horizon sleeps within violet glass, and your sight walks where your body cannot."),
            Potion(57, "Potion of Longevity", CardRarity.VeryRare, "Restoration", 10, "Years", "Sands Reversed", "Reduce Physical Age by 10 Years", "Restore Youthful Vitality", "10% Chance to Age 10 Years Instead", "Golden years flow backward through living glass, and time loosens its grasp upon you."),
            Potion(58, "Potion of Lightning Resistance", CardRarity.Uncommon, "Protection", 50, "Lightning Resistance", "Storm Ward", "Halves Lightning Damage for 5 Turns", "Immune to Shocked", "-2 Thunder Resistance While Active", "A captive storm turns within enchanted glass; lightning may strike, but cannot claim your heart."),
            Potion(59, "Potion of Healing", CardRarity.Common, "Healing", 10, "Healing", "Restorative Draught", "Restores 20 Health Instantly", "Removes Bleeding", "Cannot Exceed Maximum Health", "A warm crimson draught that closes wounds and restores the strength to rise again."),
            Potion(60, "Potion of Greater Healing", CardRarity.Uncommon, "Healing", 20, "Healing", "Greater Restoration", "Restores 40 Health Instantly", "Removes Bleeding and Poison", "Cannot Exceed Maximum Health", "A concentrated crimson remedy that mends even the wounds that refuse to close.")
        ]);
    }

    private static Card Card(int id, string name, CardType type, CardRarity rarity, string cardClass, int damage, string? damageType,
        int cost = 0, int? power = null, string? powerType = null, int? defense = null, string? defenseType = null, string? range = null, string? weight = null,
        string? charges = null, string? activation = null, string active = "", string passive = "", string bonus = "", string penalty = "", string lore = "") => new()
    {
        Id = id, Name = name, CardType = type, Rarity = rarity, Category = ResolveCategory(type, powerType), CardClass = cardClass, BaseDamage = damage, DamageType = damageType,
        BaseCost = cost, Power = power, PowerType = powerType, Defense = defense, DefenseType = defenseType, Range = range, Weight = weight,
        Charges = charges, Activation = activation, ActiveEffect = active, PassiveEffect = passive, Bonus = bonus, Penalty = penalty, Lore = lore,
        Description = lore,
        TargetType = range switch
        {
            "Party" => TargetType.AllAllies,
            "All Enemies" => TargetType.AllEnemies,
            "Self" => TargetType.Self,
            _ => TargetType.None
        },
        EffectType = ResolveEffectType(damage, damageType, powerType, active)
    };

    private static CardCategory ResolveCategory(CardType type, string? powerType) => type switch
    {
        CardType.Weapon => CardCategory.Offensive,
        CardType.Armor => CardCategory.Defensive,
        CardType.Consumable when powerType == "Healing" => CardCategory.Healing,
        CardType.Consumable => CardCategory.Utility,
        _ => CardCategory.Utility
    };

    private static EffectType ResolveEffectType(int damage, string? damageType, string? powerType, string active)
    {
        if (damage > 0 || damageType is not null) return EffectType.Damage;
        if (powerType == "Healing") return EffectType.Healing;
        if (powerType == "Strength") return EffectType.Strength;
        if (powerType == "Poison") return EffectType.StatusEffect;
        if (active.Contains("Ward", StringComparison.OrdinalIgnoreCase) || active is "Stoneheart" or "Lunar Veil") return EffectType.Protect;
        return EffectType.Custom;
    }

    private static Card Artifact(int id, string name, CardRarity rarity, string cardClass, int? power = null, string? powerType = null, int? defense = null,
        string? defenseType = null, string? charges = null, string? range = null, string? activation = null, string active = "", string passive = "", string bonus = "", string penalty = "", string lore = "", int cost = 0) =>
        Card(id, name, CardType.Artifact, rarity, cardClass, 0, null, cost, power, powerType, defense, defenseType, range, null, charges, activation, active, passive, bonus, penalty, lore);

    private static Card Consumable(int id, string name, CardRarity rarity, string cardClass, int? power = null, string? powerType = null, string? charges = null,
        string? range = null, string active = "", string passive = "", string bonus = "", string penalty = "", string lore = "") =>
        Card(id, name, CardType.Consumable, rarity, cardClass, 0, null, power: power, powerType: powerType, range: range, charges: charges, active: active, passive: passive, bonus: bonus, penalty: penalty, lore: lore);

    private static Card Potion(int id, string name, CardRarity rarity, string cardClass, int power, string powerType, string active, string passive, string bonus, string penalty, string lore) =>
        Consumable(id, name, rarity, $"Alchemy / {cardClass}", power, powerType, "Single Use", "Self", active, passive, bonus, penalty, lore);
}
