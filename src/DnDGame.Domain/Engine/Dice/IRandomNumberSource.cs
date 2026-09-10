namespace DnDGame.Domain.Engine.Dice;

/// <summary>
/// Injectable source of random integer values, enabling deterministic dice tests.
/// </summary>
public interface IRandomNumberSource
{
    int Next(int minimumInclusive, int maximumExclusive);
}
