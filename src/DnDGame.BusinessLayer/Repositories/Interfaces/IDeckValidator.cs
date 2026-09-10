namespace DnDGame.BusinessLayer.Repositories.Interfaces;

using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Entities.Game;

/// <summary>
/// Interface for validating deck composition against configurable rules.
/// </summary>
public interface IDeckValidator
{
    /// <summary>
    /// Validates a deck against the configured deck rules.
    /// Checks minimum/maximum deck size, card copy limits, and card validity.
    /// </summary>
    /// <param name="deck">The deck to validate.</param>
    /// <returns>An EngineResult indicating success or failure with appropriate error code.</returns>
    EngineResult ValidateDeck(Deck deck);
}
