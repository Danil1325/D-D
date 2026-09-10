namespace DnDGame.BusinessLayer.Effects;

using DnDGame.BusinessLayer.Effects.Interfaces;

/// <summary>
/// Registry for card effects using the Strategy Pattern.
/// Manages effect instances by name without using switch statements or if-else chains.
/// Enables adding new effects without modifying existing code (Open/Closed Principle).
/// </summary>
public class CardEffectRegistry
{
    private readonly Dictionary<string, ICardEffect> _effects;

    /// <summary>
    /// Creates a new CardEffectRegistry with no effects.
    /// Call Register() to add effect strategies.
    /// </summary>
    public CardEffectRegistry()
    {
        _effects = new Dictionary<string, ICardEffect>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a new CardEffectRegistry with initial effects.
    /// </summary>
    /// <param name="effects">Collection of effects to register initially.</param>
    public CardEffectRegistry(IEnumerable<ICardEffect> effects) : this()
    {
        foreach (var effect in effects ?? Enumerable.Empty<ICardEffect>())
        {
            if (effect != null)
            {
                Register(effect);
            }
        }
    }

    /// <summary>
    /// Registers a new effect strategy by name.
    /// If an effect with the same name already exists, it will be overwritten.
    /// </summary>
    /// <param name="effect">The effect strategy to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when effect is null.</exception>
    public void Register(ICardEffect effect)
    {
        if (effect == null)
        {
            throw new ArgumentNullException(nameof(effect), "Effect cannot be null.");
        }

        _effects[effect.EffectName] = effect;
    }

    /// <summary>
    /// Registers multiple effect strategies.
    /// </summary>
    /// <param name="effects">Collection of effect strategies to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when effects is null.</exception>
    public void RegisterMany(IEnumerable<ICardEffect> effects)
    {
        if (effects == null)
        {
            throw new ArgumentNullException(nameof(effects));
        }

        foreach (var effect in effects)
        {
            Register(effect);
        }
    }

    /// <summary>
    /// Gets an effect strategy by name.
    /// </summary>
    /// <param name="effectName">The name of the effect to retrieve.</param>
    /// <returns>The effect strategy if found, null otherwise.</returns>
    public ICardEffect? GetEffect(string effectName)
    {
        if (string.IsNullOrWhiteSpace(effectName))
        {
            return null;
        }

        _effects.TryGetValue(effectName, out var effect);
        return effect;
    }

    /// <summary>
    /// Checks if an effect with the given name is registered.
    /// </summary>
    /// <param name="effectName">The name of the effect to check.</param>
    /// <returns>True if the effect is registered, false otherwise.</returns>
    public bool HasEffect(string effectName)
    {
        return !string.IsNullOrWhiteSpace(effectName) && _effects.ContainsKey(effectName);
    }

    /// <summary>
    /// Gets all registered effect names.
    /// </summary>
    /// <returns>A collection of all registered effect names.</returns>
    public IEnumerable<string> GetRegisteredEffectNames()
    {
        return _effects.Keys.AsEnumerable();
    }

    /// <summary>
    /// Gets all registered effect strategies.
    /// </summary>
    /// <returns>A collection of all registered effect strategies.</returns>
    public IEnumerable<ICardEffect> GetAllEffects()
    {
        return _effects.Values.AsEnumerable();
    }

    /// <summary>
    /// Gets the count of registered effects.
    /// </summary>
    public int Count => _effects.Count;

    /// <summary>
    /// Unregisters an effect by name.
    /// </summary>
    /// <param name="effectName">The name of the effect to unregister.</param>
    /// <returns>True if the effect was unregistered, false if not found.</returns>
    public bool Unregister(string effectName)
    {
        if (string.IsNullOrWhiteSpace(effectName))
        {
            return false;
        }

        return _effects.Remove(effectName);
    }

    /// <summary>
    /// Clears all registered effects.
    /// </summary>
    public void Clear()
    {
        _effects.Clear();
    }
}
