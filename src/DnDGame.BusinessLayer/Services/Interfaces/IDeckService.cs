using DnDGame.BusinessLayer.Dtos.Decks;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>CRUD over the current player's decks, plus deck-rule validation.</summary>
public interface IDeckService
{
    Task<IReadOnlyList<DeckResponseDto>> GetMyDecksAsync();

    /// <summary>Throws DomainException(NOT_FOUND) if the deck doesn't exist or isn't owned by the current player.</summary>
    Task<DeckResponseDto> GetByIdAsync(int id);

    Task<DeckResponseDto> CreateAsync(DeckSaveRequestDto request);

    Task<DeckResponseDto> UpdateAsync(int id, DeckSaveRequestDto request);

    Task DeleteAsync(int id);

    Task<DeckValidationResultDto> ValidateAsync(int id);
}
