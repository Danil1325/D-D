using System.Globalization;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Shop;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Shop;

namespace DnDGame.BusinessLayer.Services;

/// <summary>Application-service implementation of the Shop catalog and buy/sell flow. See <see cref="IShopService"/>.</summary>
public class ShopService : IShopService
{
    private readonly IShopItemRepository _shopItemRepository;
    private readonly ICharacterInventoryRepository _inventoryRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICurrentPlayerService _currentPlayerService;

    public ShopService(
        IShopItemRepository shopItemRepository,
        ICharacterInventoryRepository inventoryRepository,
        ICharacterRepository characterRepository,
        ICurrentPlayerService currentPlayerService)
    {
        _shopItemRepository = shopItemRepository;
        _inventoryRepository = inventoryRepository;
        _characterRepository = characterRepository;
        _currentPlayerService = currentPlayerService;
    }

    public async Task<IReadOnlyList<ShopItemDto>> GetCatalogAsync()
    {
        var items = await _shopItemRepository.GetAllAsync();
        return items
            .OrderBy(item => item.Id)
            .Select(ShopItemDto.FromDomain)
            .ToList();
    }

    public async Task<ShopStateDto> GetStateForCurrentPlayerAsync()
    {
        var character = await ResolveCurrentCharacterAsync();
        return await BuildShopStateAsync(character);
    }

    public async Task<ShopStateDto> BuyItemForCurrentPlayerAsync(int itemId)
    {
        var character = await ResolveCurrentCharacterAsync();

        var item = await _shopItemRepository.GetByIdAsync(itemId);
        if (item is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Shop item {itemId} was not found.");
        }

        if (character.Gold < item.BuyPrice)
        {
            throw new DomainException(ShopErrorCodes.InsufficientGold, $"Character {character.Id} does not have enough gold to buy item {itemId}.");
        }

        character.Gold -= item.BuyPrice;
        await _characterRepository.UpdateAsync(character);

        var existing = await _inventoryRepository.GetAsync(character.Id, itemId);
        if (existing is null)
        {
            await _inventoryRepository.AddAsync(new CharacterInventoryEntry
            {
                PlayerId = character.Id,
                ShopItemId = itemId,
                Quantity = 1
            });
        }
        else
        {
            existing.Quantity += 1;
            await _inventoryRepository.UpdateAsync(existing);
        }

        return await BuildShopStateAsync(character);
    }

    public async Task<ShopStateDto> SellItemsForCurrentPlayerAsync(IReadOnlyList<SellItemRequestLineDto> items)
    {
        if (items is null || items.Count == 0)
        {
            throw new DomainException(ErrorCodes.ValidationError, "At least one item must be selected to sell.");
        }

        var character = await ResolveCurrentCharacterAsync();
        var inventory = await _inventoryRepository.GetByPlayerIdAsync(character.Id);
        var inventoryByItemId = inventory.ToDictionary(entry => entry.ShopItemId);

        // Merge duplicate lines for the same item, mirroring the frontend's
        // quantitiesToSell Map (src/pages/Shop/Shop.tsx playerReducer).
        var quantitiesToSell = new Dictionary<int, int>();
        foreach (var line in items)
        {
            if (line.Quantity <= 0)
            {
                throw new DomainException(ErrorCodes.ValidationError, "Each item's quantity must be positive.");
            }

            quantitiesToSell[line.ItemId] = quantitiesToSell.GetValueOrDefault(line.ItemId) + line.Quantity;
        }

        var totalValue = 0;
        var resolvedSales = new List<(CharacterInventoryEntry Entry, int Quantity)>();
        foreach (var (itemId, quantity) in quantitiesToSell)
        {
            if (!inventoryByItemId.TryGetValue(itemId, out var entry) || quantity > entry.Quantity)
            {
                throw new DomainException(ShopErrorCodes.InsufficientItemQuantity, $"Character {character.Id} does not own enough of item {itemId} to sell {quantity}.");
            }

            var shopItem = await _shopItemRepository.GetByIdAsync(itemId)
                ?? throw new DomainException(ErrorCodes.NotFound, $"Shop item {itemId} was not found.");

            totalValue += shopItem.SellPrice * quantity;
            resolvedSales.Add((entry, quantity));
        }

        foreach (var (entry, quantity) in resolvedSales)
        {
            entry.Quantity -= quantity;
            await _inventoryRepository.UpdateAsync(entry);
        }

        character.Gold += totalValue;
        await _characterRepository.UpdateAsync(character);

        return await BuildShopStateAsync(character);
    }

    private async Task<PlayerCharacter> ResolveCurrentCharacterAsync()
    {
        var currentPlayerId = _currentPlayerService.GetCurrentPlayerId();
        var ownerId = currentPlayerId.ToString(CultureInfo.InvariantCulture);
        var characters = await _characterRepository.GetAllAsync();
        var character = characters.FirstOrDefault(candidate => candidate.OwnerId == ownerId);
        if (character is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"No character was found for player {currentPlayerId}.");
        }

        return character;
    }

    private async Task<ShopStateDto> BuildShopStateAsync(PlayerCharacter character)
    {
        var inventory = await _inventoryRepository.GetByPlayerIdAsync(character.Id);
        var items = await _shopItemRepository.GetAllAsync();
        var itemsById = items.ToDictionary(item => item.Id);

        var entries = inventory
            .Where(entry => entry.Quantity > 0)
            .Select(entry => InventoryEntryDto.FromDomain(itemsById[entry.ShopItemId], entry.Quantity))
            .OrderBy(entry => entry.Name)
            .ToList();

        return new ShopStateDto
        {
            PlayerId = character.Id,
            Gold = character.Gold,
            Inventory = entries
        };
    }
}
