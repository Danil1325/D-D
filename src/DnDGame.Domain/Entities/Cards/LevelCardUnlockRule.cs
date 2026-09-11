using DnDGame.Domain.Entities.Characters;

namespace DnDGame.Domain.Entities.Cards;

/// <summary>Unlocks a card once the character has reached a required level.</summary>
public sealed class LevelCardUnlockRule : ICardUnlockRule
{
    public LevelCardUnlockRule(int cardId, int requiredLevel)
    {
        CardId = cardId;
        RequiredLevel = requiredLevel;
    }

    public int CardId { get; }

    public int RequiredLevel { get; }

    public bool IsValid => CardId > 0 && RequiredLevel > 0;

    public bool IsSatisfiedBy(PlayerCharacter playerCharacter)
    {
        ArgumentNullException.ThrowIfNull(playerCharacter);
        return IsValid && playerCharacter.Level >= RequiredLevel;
    }
}
