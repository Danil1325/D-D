using DnDGame.BusinessLayer.Dtos.Shop;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

/// <summary>
/// Shop endpoints: a not-player-scoped catalog, plus the current player's gold
/// balance/inventory and the buy/sell actions that mutate it. See IShopService
/// for why this is a Shop-only item model with no stock limit.
/// </summary>
[ApiController]
[Route("api/shop")]
public class ShopController : ControllerBase
{
    private readonly IShopService _shopService;

    public ShopController(IShopService shopService)
    {
        _shopService = shopService;
    }

    /// <summary>The whole Shop item catalog.</summary>
    [HttpGet("catalog")]
    public async Task<ActionResult<IReadOnlyList<ShopItemDto>>> GetCatalog()
    {
        var result = await _shopService.GetCatalogAsync();
        return Ok(result);
    }

    /// <summary>
    /// The current player's gold balance and owned items. 404 (via the global
    /// middleware's NOT_FOUND mapping) when the current player has no character
    /// yet — consistent with GET /api/character/current.
    /// </summary>
    [HttpGet("current")]
    public async Task<ActionResult<ShopStateDto>> GetCurrent()
    {
        var result = await _shopService.GetStateForCurrentPlayerAsync();
        return Ok(result);
    }

    /// <summary>Buys one unit of an item for the current player and returns their updated state.</summary>
    [HttpPost("items/{id:int}/buy")]
    public async Task<ActionResult<ShopStateDto>> Buy(int id)
    {
        var result = await _shopService.BuyItemForCurrentPlayerAsync(id);
        return Ok(result);
    }

    /// <summary>Sells one or more items for the current player in a single batch and returns their updated state.</summary>
    [HttpPost("sell")]
    public async Task<ActionResult<ShopStateDto>> Sell([FromBody] SellItemsRequestDto request)
    {
        var result = await _shopService.SellItemsForCurrentPlayerAsync(request.Items);
        return Ok(result);
    }
}
