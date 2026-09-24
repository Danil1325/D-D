using DnDGame.API.Controllers;
using DnDGame.BusinessLayer.Dtos.Shop;
using DnDGame.BusinessLayer.Services;
using DnDGame.Domain.Entities.Characters;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using DnDGame.MockData.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DnDGame.Tests.API;

public class ShopControllerTests
{
    [Fact]
    public async Task GetCatalog_ReturnsTheWholeSeededCatalog()
    {
        var (service, _) = CreateServiceWithCharacter();
        var controller = new ShopController(service);

        var response = await controller.GetCatalog();

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var catalog = Assert.IsAssignableFrom<IReadOnlyList<ShopItemDto>>(result.Value);
        Assert.Equal(60, catalog.Count);
    }

    [Fact]
    public async Task Buy_ThenSell_RoundTripsGoldAndInventory()
    {
        var (service, store) = CreateServiceWithCharacter(gold: 1000);
        var item = store.ShopItems.First(i => i.Code == "weapons-the-sword");
        var controller = new ShopController(service);

        var buyResponse = await controller.Buy(item.Id);
        var buyResult = Assert.IsType<OkObjectResult>(buyResponse.Result);
        var stateAfterBuy = Assert.IsType<ShopStateDto>(buyResult.Value);
        Assert.Equal(1000 - item.BuyPrice, stateAfterBuy.Gold);

        var sellResponse = await controller.Sell(new SellItemsRequestDto
        {
            Items = new[] { new SellItemRequestLineDto { ItemId = item.Id, Quantity = 1 } }
        });
        var sellResult = Assert.IsType<OkObjectResult>(sellResponse.Result);
        var stateAfterSell = Assert.IsType<ShopStateDto>(sellResult.Value);
        Assert.Equal(1000 - item.BuyPrice + item.SellPrice, stateAfterSell.Gold);
        Assert.Empty(stateAfterSell.Inventory);
    }

    private static (ShopService Service, InMemoryGameDataStore Store) CreateServiceWithCharacter(int gold = 1000)
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var currentPlayer = new MockCurrentPlayerService();
        store.Characters.Add(new PlayerCharacter
        {
            Id = 1,
            OwnerId = currentPlayer.GetCurrentPlayerId().ToString(),
            Name = "Hero",
            Level = 1,
            RaceId = 1,
            ClassId = 1,
            Gold = gold
        });

        var service = new ShopService(
            new MockShopItemRepository(store),
            new MockCharacterInventoryRepository(store),
            new MockCharacterRepository(store),
            currentPlayer);

        return (service, store);
    }
}
