namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Calculates damage for a single hit without applying it to battle state.
/// </summary>
public interface IDamageCalculator
{
    DamageResult Calculate(int attack, int defense, int block);
}
