using DnDGame.Domain.Engine.Enums;

namespace DnDGame.Domain.Engine.Models;

/// <summary>
/// Describes an effect currently active during a battle.
/// Effect resolution is intentionally owned by a future effect engine.
/// </summary>
public abstract class ActiveEffect
{
    protected ActiveEffect(
        string type,
        int value,
        int duration,
        int stackCount,
        EffectTarget target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Effect value cannot be negative.");
        }

        if (duration < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Effect duration cannot be negative.");
        }

        if (stackCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(stackCount), "An effect must have at least one stack.");
        }

        Type = type;
        Value = value;
        Duration = duration;
        StackCount = stackCount;
        Target = target;
    }

    /// <summary>Unique identity for this application of an effect.</summary>
    public Guid EffectInstanceId { get; } = Guid.NewGuid();

    /// <summary>Stable discriminator used by an effect engine, for example <c>Burn</c>.</summary>
    public string Type { get; }

    /// <summary>Magnitude applied by one stack when the effect is resolved.</summary>
    public int Value { get; }

    /// <summary>Number of turn-boundary resolutions remaining.</summary>
    public int Duration { get; private set; }

    /// <summary>Current number of stacks for this effect.</summary>
    public int StackCount { get; private set; }

    /// <summary>Combatant that receives the effect when it is resolved.</summary>
    public EffectTarget Target { get; }

    /// <summary>Whether this effect has no remaining turn-boundary resolutions.</summary>
    public bool IsExpired => Duration == 0;

    /// <summary>
    /// Consumes one scheduled resolution. Turn processing can call this after
    /// resolving the effect, without needing to know its concrete type.
    /// </summary>
    public void ConsumeDuration()
    {
        if (Duration > 0)
        {
            Duration--;
        }
    }

    /// <summary>Changes the stack count while preserving the effect invariants.</summary>
    public void SetStackCount(int stackCount)
    {
        if (stackCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(stackCount), "An effect must have at least one stack.");
        }

        StackCount = stackCount;
    }
}
