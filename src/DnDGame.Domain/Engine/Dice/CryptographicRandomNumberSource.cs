using System.Security.Cryptography;

namespace DnDGame.Domain.Engine.Dice;

/// <summary>
/// Cryptographically secure production source for dice values.
/// </summary>
public sealed class CryptographicRandomNumberSource : IRandomNumberSource
{
    public int Next(int minimumInclusive, int maximumExclusive)
    {
        return RandomNumberGenerator.GetInt32(minimumInclusive, maximumExclusive);
    }
}
