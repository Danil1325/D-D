using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Shop;

/// <summary>
/// Reference data — one buyable/sellable item in the Shop, seeded once at
/// startup like Card/Achievement and never created or edited by players.
///
/// <see cref="Code"/> is the stable identifier the frontend already uses for the
/// same item (e.g. "weapons-the-sword", built from its asset filename in
/// src/data/marketItems.ts / src/assets/items), kept so both sides stay keyed
/// the same way. This is deliberately a Shop-only model — Collection and the
/// separate Inventory screen each have their own, currently-incompatible item
/// shape on the frontend; unifying them is a bigger cross-feature decision left
/// for later, not invented here.
/// </summary>
public class ShopItem
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ShopItemCategory Category { get; set; }

    public int BuyPrice { get; set; }

    public int SellPrice { get; set; }
}
