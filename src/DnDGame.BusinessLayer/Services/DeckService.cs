using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Decks;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.BusinessLayer.Validation;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Services;

public class DeckService : IDeckService
{
    private readonly IDeckRepository _deckRepository;
    private readonly IDeckValidator _deckValidator;
    private readonly ICurrentPlayerService _currentPlayerService;

    public DeckService(
        IDeckRepository deckRepository,
        IDeckValidator deckValidator,
        ICurrentPlayerService currentPlayerService)
    {
        _deckRepository = deckRepository;
        _deckValidator = deckValidator;
        _currentPlayerService = currentPlayerService;
    }

    public async Task<IReadOnlyList<DeckResponseDto>> GetMyDecksAsync()
    {
        var characterId = _currentPlayerService.GetCurrentPlayerId();
        var decks = await _deckRepository.GetAllByCharacterIdAsync(characterId);
        return decks.Select(DeckResponseDto.FromDomain).ToList();
    }

    public async Task<DeckResponseDto> GetByIdAsync(int id)
    {
        var deck = await GetOwnedDeckOrThrowAsync(id);
        return DeckResponseDto.FromDomain(deck);
    }

    public async Task<DeckResponseDto> CreateAsync(DeckSaveRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateName(request.Name);

        var characterId = _currentPlayerService.GetCurrentPlayerId();
        var cards = await ResolveCardsAsync(request.CardIds);

        var deck = new Deck
        {
            Name = request.Name,
            Description = request.Description,
            CharacterId = characterId,
            Cards = cards.ToList()
        };

        var created = await _deckRepository.AddAsync(deck);
        return DeckResponseDto.FromDomain(created);
    }

    public async Task<DeckResponseDto> UpdateAsync(int id, DeckSaveRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateName(request.Name);

        var deck = await GetOwnedDeckOrThrowAsync(id);
        var cards = await ResolveCardsAsync(request.CardIds);

        deck.Name = request.Name;
        deck.Description = request.Description;
        deck.Cards = cards.ToList();

        await _deckRepository.UpdateAsync(deck);
        return DeckResponseDto.FromDomain(deck);
    }

    public async Task DeleteAsync(int id)
    {
        await GetOwnedDeckOrThrowAsync(id);
        await _deckRepository.DeleteAsync(id);
    }

    public async Task<DeckValidationResultDto> ValidateAsync(int id)
    {
        var deck = await GetOwnedDeckOrThrowAsync(id);
        var result = _deckValidator.ValidateDeck(deck);
        return DeckValidationResultDto.FromEngineResult(result);
    }

    private static void ValidateName(string? name)
    {
        var validation = RequestValidationHelpers.RequireNonEmpty(name, "Name");
        if (!validation.IsValid)
        {
            throw new DomainException(ErrorCodes.ValidationError, string.Join(" ", validation.Errors));
        }
    }

    private async Task<IReadOnlyList<Card>> ResolveCardsAsync(IReadOnlyCollection<int> cardIds)
    {
        var distinctIds = cardIds.Distinct().ToList();
        var cards = await _deckRepository.GetCardsByIdsAsync(distinctIds);
        if (cards.Count != distinctIds.Count)
        {
            var missing = distinctIds.Except(cards.Select(card => card.Id));
            throw new DomainException(
                ErrorCodes.ValidationError,
                $"Unknown card id(s): {string.Join(", ", missing)}.");
        }

        return cards;
    }

    private async Task<Deck> GetOwnedDeckOrThrowAsync(int id)
    {
        var validation = RequestValidationHelpers.RequirePositiveId(id, "id");
        if (!validation.IsValid)
        {
            throw new DomainException(ErrorCodes.ValidationError, string.Join(" ", validation.Errors));
        }

        var characterId = _currentPlayerService.GetCurrentPlayerId();
        var deck = await _deckRepository.GetByIdAsync(id);
        if (deck is null || deck.CharacterId != characterId)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Deck {id} was not found.");
        }

        return deck;
    }
}
