namespace DnDGame.Domain.Enums;

/// <summary>
/// The Collection screen's browsing category, mirroring the frontend's
/// Category union ('Heroes' | 'Classes' | 'Enemies' | 'Weapons' | 'Armor' |
/// 'Potions'). Presentation grouping only.
/// </summary>
public enum CollectionCategory
{
    Heroes = 1,
    Classes = 2,
    Enemies = 3,
    Weapons = 4,
    Armor = 5,
    Potions = 6
}
