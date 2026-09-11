using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Decks;
using DnDGame.BusinessLayer.Repositories;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;

namespace DnDGame.Tests.BusinessLayer;

public class DeckServiceTests
{
    private const int CurrentPlayerId = 1;
    private const int OtherPlayerId = 2;

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsAndReturnsDeck()
    {
        var (service, store) = CreateService();
        AddCatalogueCards(store, 1, 2, 3);

        var result = await service.CreateAsync(new DeckSaveRequestDto
        {
            Name = "Fire Deck",
            Description = "Burn it down",
            CardIds = [1, 2, 3]
        });

        Assert.NotEqual(0, result.Id);
        Assert.Equal("Fire Deck", result.Name);
        Assert.Equal(CurrentPlayerId, result.CharacterId);
        Assert.Equal([1, 2, 3], result.CardIds);
    }

    [Fact]
    public async Task CreateAsync_BlankName_ThrowsValidationError()
    {
        var (service, _) = CreateService();

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.CreateAsync(new DeckSaveRequestDto { Name = " " }));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_UnknownCardId_ThrowsValidationError()
    {
        var (service, store) = CreateService();
        AddCatalogueCards(store, 1);

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.CreateAsync(new DeckSaveRequestDto { Name = "Deck", CardIds = [1, 999] }));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
        Assert.Contains("999", exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_DeckOwnedByAnotherPlayer_ThrowsNotFound()
    {
        var (service, store) = CreateService();
        var deck = await new MockDeckRepository(store).AddAsync(new Deck
        {
            Name = "Not mine",
            CharacterId = OtherPlayerId
        });

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.GetByIdAsync(deck.Id));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_OwnedDeck_ReplacesFields()
    {
        var (service, store) = CreateService();
        AddCatalogueCards(store, 1, 2);
        var created = await service.CreateAsync(new DeckSaveRequestDto { Name = "Original", CardIds = [1] });

        var updated = await service.UpdateAsync(created.Id, new DeckSaveRequestDto
        {
            Name = "Renamed",
            Description = "New description",
            CardIds = [2]
        });

        Assert.Equal("Renamed", updated.Name);
        Assert.Equal("New description", updated.Description);
        Assert.Equal([2], updated.CardIds);
    }

    [Fact]
    public async Task DeleteAsync_OwnedDeck_RemovesIt()
    {
        var (service, _) = CreateService();
        var created = await service.CreateAsync(new DeckSaveRequestDto { Name = "Doomed" });

        await service.DeleteAsync(created.Id);

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetByIdAsync(created.Id));
        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_DeckBelowMinimumSize_ReturnsInvalidWithDeckTooSmall()
    {
        var (service, store) = CreateService();
        AddCatalogueCards(store, 1);
        var created = await service.CreateAsync(new DeckSaveRequestDto { Name = "Tiny", CardIds = [1] });

        var result = await service.ValidateAsync(created.Id);

        Assert.False(result.IsValid);
        Assert.Equal(nameof(ErrorCode.DECK_TOO_SMALL), result.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_DeckWithinSizeRules_ReturnsValid()
    {
        var (service, store) = CreateServiceWithRules(new DeckRules(minimumDeckSize: 1, maximumDeckSize: 5, maximumCopiesPerCard: 4));
        AddCatalogueCards(store, 1, 2);
        var created = await service.CreateAsync(new DeckSaveRequestDto { Name = "Small but valid", CardIds = [1, 2] });

        var result = await service.ValidateAsync(created.Id);

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorCode);
    }

    private static (IDeckService Service, InMemoryGameDataStore Store) CreateService()
    {
        return CreateServiceWithRules(new DeckRules(minimumDeckSize: 20, maximumDeckSize: 40, maximumCopiesPerCard: 4));
    }

    private static (IDeckService Service, InMemoryGameDataStore Store) CreateServiceWithRules(DeckRules rules)
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockDeckRepository(store);
        var validator = new DeckValidator(rules);
        var service = new DeckService(repository, validator, new FixedCurrentPlayerService(CurrentPlayerId));
        return (service, store);
    }

    private static void AddCatalogueCards(InMemoryGameDataStore store, params int[] ids)
    {
        foreach (var id in ids)
        {
            store.Cards.Add(new Card { Id = id, Name = $"Card {id}" });
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
