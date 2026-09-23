using DnDGame.Domain.Entities.Races;

namespace DnDGame.BusinessLayer.Dtos.Characters;

/// <summary>
/// The min/max roll range for one attribute of one race. The attribute name is
/// serialized as text (e.g. "Health", "Strength") for the frontend.
/// </summary>
public class AttributeRangeDto
{
    public string Attribute { get; init; } = string.Empty;
    public int MinValue { get; init; }
    public int MaxValue { get; init; }

    public static AttributeRangeDto FromDomain(RaceAttributeRange range) => new()
    {
        Attribute = range.Attribute.ToString(),
        MinValue = range.MinValue,
        MaxValue = range.MaxValue
    };
}