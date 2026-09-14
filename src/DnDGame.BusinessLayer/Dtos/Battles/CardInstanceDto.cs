using DnDGame.Domain.Entities.Cards;

namespace DnDGame.BusinessLayer.Dtos.Battles;

/// <summary>
/// Response shape for one card instance in a hand/pile. Exposes only the id and
/// card definition id — same convention as DeckResponseDto.CardIds — so clients
/// resolve display details via the existing Card feature instead of duplicating
/// card data here.
/// </summary>
public class CardInstanceDto
{
    public Guid InstanceId { get; init; }
    public int CardId { get; init; }

    public static CardInstanceDto FromDomain(CardInstance card) => new()
    {
        InstanceId = card.InstanceId,
        CardId = card.CardId
    };
}
