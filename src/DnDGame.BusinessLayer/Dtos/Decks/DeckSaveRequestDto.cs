namespace DnDGame.BusinessLayer.Dtos.Decks;

/// <summary>Request shape for both creating and updating a deck.</summary>
public class DeckSaveRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<int> CardIds { get; set; } = new();
}
