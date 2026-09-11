using DnDGame.Domain.Entities.Cards;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

public interface ICardCollectionRepository
{
    /// <summary>
    /// Returns the player character's card collection, creating an empty one if
    /// this is their first card-related request. Mirrors how a new player would
    /// implicitly get an empty collection in a real database via a default row.
    /// </summary>
    Task<CardCollection> GetOrCreateByPlayerCharacterIdAsync(int playerCharacterId);
}
