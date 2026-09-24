namespace DnDGame.Domain.Entities.Shop;

/// <summary>
/// How many of one ShopItem a character owns. Composite-keyed
/// (PlayerId, ShopItemId), mirroring how AchievementProgress and
/// CharacterSkillUnlock are keyed — one row per player per item, created on
/// first purchase and updated by later buys/sells.
///
/// A row with Quantity 0 is left in place rather than deleted (no delete
/// operation exists on ICharacterInventoryRepository, matching the other
/// per-player tables' "update in place" convention); ShopService filters those
/// out when building the player-facing inventory view.
/// </summary>
public class CharacterInventoryEntry
{
    public int PlayerId { get; set; }
    public int ShopItemId { get; set; }
    public int Quantity { get; set; }
}
