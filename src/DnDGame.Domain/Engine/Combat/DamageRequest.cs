using DnDGame.Domain.Engine.Dice;

namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Immutable input for a deterministic damage calculation. Any required dice roll
/// must already have been resolved by IDiceEngine.
/// </summary>
public sealed class DamageRequest
{
    public DamageRequest(
        int baseDamage,
        int strength,
        int defense,
        int block,
        IEnumerable<int>? buffModifiers = null,
        IEnumerable<int>? debuffModifiers = null,
        DiceResult? diceResult = null)
    {
        BaseDamage = baseDamage;
        Strength = strength;
        Defense = defense;
        Block = block;
        BuffModifiers = (buffModifiers ?? Array.Empty<int>()).ToArray();
        DebuffModifiers = (debuffModifiers ?? Array.Empty<int>()).ToArray();
        DiceResult = diceResult;
    }

    public int BaseDamage { get; }
    public int Strength { get; }
    public int Defense { get; }
    public int Block { get; }
    public IReadOnlyList<int> BuffModifiers { get; }
    public IReadOnlyList<int> DebuffModifiers { get; }
    public DiceResult? DiceResult { get; }
}
