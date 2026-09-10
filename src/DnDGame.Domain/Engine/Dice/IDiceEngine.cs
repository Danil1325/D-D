namespace DnDGame.Domain.Engine.Dice;

/// <summary>
/// Contract for obtaining a dice result.
/// </summary>
public interface IDiceEngine
{
    int Roll(int sides);
}
