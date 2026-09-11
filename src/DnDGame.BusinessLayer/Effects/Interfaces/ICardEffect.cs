namespace DnDGame.BusinessLayer.Effects.Interfaces;

/// <summary>
/// Strategy interface for card effects.
/// Represents a single effect that a card can apply during battle.
/// Implementations of this interface can be easily added without modifying existing code.
/// 
/// Design Pattern: Strategy
/// Each effect is a separate strategy that encapsulates a specific behavior.
/// </summary>
public interface ICardEffect
{
    /// <summary>
    /// Gets the unique name/identifier for this effect type.
    /// Used to match effects to cards and lookup strategies.
    /// Examples: "Damage", "Heal", "Poison", "Stun", "Buff"
    /// </summary>
    string EffectName { get; }

    /// <summary>
    /// Determines if this effect can be applied in the current battle context.
    /// Allows effects to be conditional based on battle state, targets, modifiers, etc.
    /// Called before Apply() to validate applicability.
    /// </summary>
    /// <param name="context">The battle context containing all state information.</param>
    /// <returns>True if this effect can be applied, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    bool CanApply(CardEffectContext context);

    /// <summary>
    /// Applies this effect to the battle state.
    /// Should only be called after CanApply() returns true.
    /// Must modify the battle state directly (no return value).
    /// </summary>
    /// <param name="context">The battle context containing all state information.</param>
    /// <returns>A description of what the effect did (for logging/UI).</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    string Apply(CardEffectContext context);
}
