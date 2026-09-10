namespace DnDGame.Domain.Engine.Models;

/// <summary>
/// Describes an effect currently active during a battle.
/// Effect resolution is intentionally owned by a future effect engine.
/// </summary>
public class ActiveEffect
{
    public Guid EffectInstanceId { get; set; }

    public string EffectKey { get; set; } = string.Empty;

    public int Stacks { get; set; }

    /// <summary>
    /// Number of turns remaining. A null value represents an effect without a
    /// turn-based expiration.
    /// </summary>
    public int? RemainingTurns { get; set; }
}
