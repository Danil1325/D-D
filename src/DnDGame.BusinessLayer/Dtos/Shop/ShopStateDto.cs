namespace DnDGame.BusinessLayer.Dtos.Shop;

/// <summary>The current player's Shop-relevant state: gold balance and owned items (quantity 0 entries omitted).</summary>
public class ShopStateDto
{
    public int PlayerId { get; init; }
    public int Gold { get; init; }
    public IReadOnlyList<InventoryEntryDto> Inventory { get; init; } = Array.Empty<InventoryEntryDto>();
}
