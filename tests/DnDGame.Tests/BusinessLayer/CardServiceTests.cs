using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Cards;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;

namespace DnDGame.Tests.BusinessLayer;

public class CardServiceTests
{
    private const int CurrentPlayerId = 1;

    [Fact]
    public async Task SearchCardsAsync_NoCardsInCollection_ReturnsEmptyPage()
    {
        var service = CreateService(out _);

        var result = await service.SearchCardsAsync(new CardSearchRequestDto());

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task SearchCardsAsync_AppliesFilterAndPaging()
    {
        var service = CreateService(out var store);
        AddCatalogueCard(store, 1, "Arcane Bolt", CardRarity.Rare);
        AddCatalogueCard(store, 2, "Bronze Shield", CardRarity.Common);
        AddCatalogueCard(store, 3, "Amber Blade", CardRarity.Rare);
        await SeedLockedCardsForCurrentPlayer(service, store, 1, 2, 3);

        var result = await service.SearchCardsAsync(new CardSearchRequestDto
        {
            Rarity = CardRarity.Rare,
            Page = 1,
            PageSize = 1
        });

        Assert.Equal(2, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.PageSize);
    }

    [Fact]
    public async Task SearchCardsAsync_InvalidPaging_ThrowsValidationError()
    {
        var service = CreateService(out _);

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.SearchCardsAsync(new CardSearchRequestDto { Page = 0 }));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
    }

    [Fact]
    public async Task GetCardByIdAsync_CardInCollection_ReturnsDto()
    {
        var service = CreateService(out var store);
        AddCatalogueCard(store, 5, "Bronze Shield", CardRarity.Common);
        await SeedLockedCardsForCurrentPlayer(service, store, 5);

        var result = await service.GetCardByIdAsync(5);

        Assert.Equal(5, result.CardId);
        Assert.Equal("Bronze Shield", result.Name);
    }

    [Fact]
    public async Task GetCardByIdAsync_CardNotInCollection_ThrowsNotFound()
    {
        var service = CreateService(out _);

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.GetCardByIdAsync(999));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task GetCardByIdAsync_NonPositiveId_ThrowsValidationError()
    {
        var service = CreateService(out _);

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.GetCardByIdAsync(0));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
    }

    private static ICardService CreateService(out InMemoryGameDataStore store)
    {
        store = new InMemoryGameDataStore();
        var repository = new MockCardCollectionRepository(store);
        return new CardService(repository, new FixedCurrentPlayerService(CurrentPlayerId));
    }

    private static void AddCatalogueCard(InMemoryGameDataStore store, int id, string name, CardRarity rarity)
    {
        store.Cards.Add(new Card { Id = id, Name = name, Rarity = rarity });
    }

    private static async Task SeedLockedCardsForCurrentPlayer(ICardService service, InMemoryGameDataStore store, params int[] cardIds)
    {
        var repository = new MockCardCollectionRepository(store);
        var collection = await repository.GetOrCreateByPlayerCharacterIdAsync(CurrentPlayerId);
        foreach (var cardId in cardIds)
        {
            collection.AddLockedCard(cardId, CardVisibilityRule.ShowBasicInformation);
        }
    }

    private sealed class FixedCurrentPlayerService : ICurrentPlayerService
    {
        private readonly int _playerId;

        public FixedCurrentPlayerService(int playerId)
        {
            _playerId = playerId;
        }

        public int GetCurrentPlayerId() => _playerId;
    }
}
