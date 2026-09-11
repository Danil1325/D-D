using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Cards;
using DnDGame.BusinessLayer.Dtos.Common;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.BusinessLayer.Validation;
using DnDGame.Domain.Entities.Cards;

namespace DnDGame.BusinessLayer.Services;

public class CardService : ICardService
{
    private readonly ICardCollectionRepository _cardCollectionRepository;
    private readonly ICurrentPlayerService _currentPlayerService;

    public CardService(
        ICardCollectionRepository cardCollectionRepository,
        ICurrentPlayerService currentPlayerService)
    {
        _cardCollectionRepository = cardCollectionRepository;
        _currentPlayerService = currentPlayerService;
    }

    public async Task<PagedResult<CardDetailsDto>> SearchCardsAsync(CardSearchRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validation = RequestValidationHelpers.ValidatePagination(request);
        if (!validation.IsValid)
        {
            throw new DomainException(ErrorCodes.ValidationError, string.Join(" ", validation.Errors));
        }

        var collection = await GetCurrentPlayerCollectionAsync();

        var options = new CardSearchOptions
        {
            Rarity = request.Rarity,
            Category = request.Category,
            Type = request.Type,
            Unlocked = request.Unlocked,
            SortBy = request.SortBy,
            SortOrder = request.SortOrder
        };

        var matches = collection.SearchCards(request.Query, options);

        var page = matches
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(CardDetailsDto.FromDomain)
            .ToList();

        return PagedResult<CardDetailsDto>.Create(page, request.Page, request.PageSize, matches.Count);
    }

    public async Task<CardDetailsDto> GetCardByIdAsync(int cardId)
    {
        var validation = RequestValidationHelpers.RequirePositiveId(cardId, nameof(cardId));
        if (!validation.IsValid)
        {
            throw new DomainException(ErrorCodes.ValidationError, string.Join(" ", validation.Errors));
        }

        var collection = await GetCurrentPlayerCollectionAsync();

        try
        {
            return CardDetailsDto.FromDomain(collection.GetCardDetails(cardId));
        }
        catch (KeyNotFoundException)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Card {cardId} was not found in the current player's collection.");
        }
    }

    private Task<CardCollection> GetCurrentPlayerCollectionAsync()
    {
        var playerCharacterId = _currentPlayerService.GetCurrentPlayerId();
        return _cardCollectionRepository.GetOrCreateByPlayerCharacterIdAsync(playerCharacterId);
    }
}
