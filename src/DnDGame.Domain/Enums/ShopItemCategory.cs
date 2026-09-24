namespace DnDGame.Domain.Enums;

/// <summary>
/// The Shop's item category, mirroring the frontend's ItemCategory union
/// ('weapons' | 'armor' | 'potions' | 'artifacts' — src/data/marketItems.ts).
/// </summary>
public enum ShopItemCategory
{
    Weapons = 1,
    Armor = 2,
    Potions = 3,
    Artifacts = 4
}
