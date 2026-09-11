using DnDGame.BusinessLayer.Dtos.Cards;
using DnDGame.BusinessLayer.Dtos.Common;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Thin application-service wrapper around the current player's CardCollection.
/// Does not reimplement filtering/sorting/visibility — that already lives on
/// CardCollection itself.
/// </summary>
public interface ICardService
{
    Task<PagedResult<CardDetailsDto>> SearchCardsAsync(CardSearchRequestDto request);

    /// <summary>Throws DomainException(NOT_FOUND) if the card isn't in the current player's collection.</summary>
    Task<CardDetailsDto> GetCardByIdAsync(int cardId);
}
