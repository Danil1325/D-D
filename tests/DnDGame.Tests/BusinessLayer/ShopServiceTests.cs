using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Shop;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Characters;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using DnDGame.MockData.Services;

namespace DnDGame.Tests.BusinessLayer;

public class ShopServiceTests
{
    private const int PlayerCharacterId = 1;

    [Fact]
    public async Task GetCatalogAsync_ReturnsTheSeededItems()
    {
        var (service, _) = CreateServiceWithCharacter();

        var catalog = await service.GetCatalogAsync();

        Assert.Equal(60, catalog.Count);
        Assert.Contains(catalog, item => item.Code == "weapons-the-sword");
    }

    [Fact]
    public async Task GetStateForCurrentPlayer_ReturnsGoldAndEmptyInventory()
    {
        var (service, _) = CreateServiceWithCharacter(gold: 500);

        var state = await service.GetStateForCurrentPlayerAsync();

        Assert.Equal(PlayerCharacterId, state.PlayerId);
        Assert.Equal(500, state.Gold);
        Assert.Empty(state.Inventory);
    }

    [Fact]
    public async Task BuyItem_DeductsGoldAndAddsToInventory()
    {
        var (service, store) = CreateServiceWithCharacter(gold: 1000);
        var item = store.ShopItems.First(i => i.Code == "weapons-the-sword");

        var state = await service.BuyItemForCurrentPlayerAsync(item.Id);

        Assert.Equal(1000 - item.BuyPrice, state.Gold);
        var entry = Assert.Single(state.Inventory, e => e.Id == item.Id);
        Assert.Equal(1, entry.Quantity);
    }

    [Fact]
    public async Task BuyItem_Twice_AccumulatesQuantity()
    {
        var (service, store) = CreateServiceWithCharacter(gold: 10_000);
        var item = store.ShopItems.First(i => i.Code == "weapons-the-sword");

        await service.BuyItemForCurrentPlayerAsync(item.Id);
        var state = await service.BuyItemForCurrentPlayerAsync(item.Id);

        var entry = Assert.Single(state.Inventory, e => e.Id == item.Id);
        Assert.Equal(2, entry.Quantity);
    }

    [Fact]
    public async Task BuyItem_WhenNotEnoughGold_ThrowsInsufficientGold()
    {
        var (service, store) = CreateServiceWithCharacter(gold: 0);
        var item = store.ShopItems.First();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.BuyItemForCurrentPlayerAsync(item.Id));

        Assert.Equal(ShopErrorCodes.InsufficientGold, exception.ErrorCode);
    }

    [Fact]
    public async Task BuyItem_WhenItemDoesNotExist_ThrowsNotFound()
    {
        var (service, _) = CreateServiceWithCharacter(gold: 10_000);

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.BuyItemForCurrentPlayerAsync(999_999));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task SellItems_CreditsGoldAndReducesQuantity()
    {
        var (service, store) = CreateServiceWithCharacter(gold: 10_000);
        var item = store.ShopItems.First(i => i.Code == "weapons-the-sword");
        await service.BuyItemForCurrentPlayerAsync(item.Id);
        await service.BuyItemForCurrentPlayerAsync(item.Id);
        var goldAfterBuying = (await service.GetStateForCurrentPlayerAsync()).Gold;

        var state = await service.SellItemsForCurrentPlayerAsync(new[]
        {
            new SellItemRequestLineDto { ItemId = item.Id, Quantity = 1 }
        });

        Assert.Equal(goldAfterBuying + item.SellPrice, state.Gold);
        var entry = Assert.Single(state.Inventory, e => e.Id == item.Id);
        Assert.Equal(1, entry.Quantity);
    }

    [Fact]
    public async Task SellItems_SellingAllRemovesTheEntryFromInventory()
    {
        var (service, store) = CreateServiceWithCharacter(gold: 10_000);
        var item = store.ShopItems.First(i => i.Code == "weapons-the-sword");
        await service.BuyItemForCurrentPlayerAsync(item.Id);

        var state = await service.SellItemsForCurrentPlayerAsync(new[]
        {
            new SellItemRequestLineDto { ItemId = item.Id, Quantity = 1 }
        });

        Assert.Empty(state.Inventory);
    }

    [Fact]
    public async Task SellItems_WhenSellingMoreThanOwned_ThrowsInsufficientQuantityAndSellsNothing()
    {
        var (service, store) = CreateServiceWithCharacter(gold: 10_000);
        var item = store.ShopItems.First(i => i.Code == "weapons-the-sword");
        await service.BuyItemForCurrentPlayerAsync(item.Id);
        var goldBeforeSell = (await service.GetStateForCurrentPlayerAsync()).Gold;

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.SellItemsForCurrentPlayerAsync(new[]
        {
            new SellItemRequestLineDto { ItemId = item.Id, Quantity = 5 }
        }));

        Assert.Equal(ShopErrorCodes.InsufficientItemQuantity, exception.ErrorCode);
        var state = await service.GetStateForCurrentPlayerAsync();
        Assert.Equal(goldBeforeSell, state.Gold);
        Assert.Single(state.Inventory, e => e.Id == item.Id && e.Quantity == 1);
    }

    [Fact]
    public async Task SellItems_WhenRequestIsEmpty_ThrowsValidationError()
    {
        var (service, _) = CreateServiceWithCharacter(gold: 10_000);

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.SellItemsForCurrentPlayerAsync(Array.Empty<SellItemRequestLineDto>()));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
    }

    private static (ShopService Service, InMemoryGameDataStore Store) CreateServiceWithCharacter(int gold = 0)
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var currentPlayer = new MockCurrentPlayerService();
        store.Characters.Add(new PlayerCharacter
        {
            Id = PlayerCharacterId,
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
