namespace DnDGame.BusinessLayer.Dtos.Shop;

/// <summary>
/// A batch sell request, matching the frontend's sell-selection panel (multiple
/// items, each with a chosen quantity, sold together — src/pages/Shop/Shop.tsx
/// handleSell).
/// </summary>
public class SellItemsRequestDto
{
    public IReadOnlyList<SellItemRequestLineDto> Items { get; init; } = Array.Empty<SellItemRequestLineDto>();
}
