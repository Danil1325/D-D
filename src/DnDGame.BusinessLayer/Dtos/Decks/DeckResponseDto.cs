using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Dtos.Decks;

public class DeckResponseDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int CharacterId { get; init; }
    public IReadOnlyList<int> CardIds { get; init; } = Array.Empty<int>();

    public static DeckResponseDto FromDomain(Deck deck) => new()
    {
        Id = deck.Id,
        Name = deck.Name,
        Description = deck.Description,
        CharacterId = deck.CharacterId,
        CardIds = deck.Cards.Select(card => card.Id).ToList()
    };
}
