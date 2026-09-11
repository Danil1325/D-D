using DnDGame.Domain.Entities.Characters;

namespace DnDGame.Domain.Entities.Cards;

/// <summary>
/// Defines a verifiable condition for unlocking a specific card.
/// Additional unlock sources (achievements, story progress, purchases) can
/// implement this contract without changing the card collection model.
/// </summary>
public interface ICardUnlockRule
{
    int CardId { get; }

    bool IsValid { get; }

    bool IsSatisfiedBy(PlayerCharacter playerCharacter);
}
