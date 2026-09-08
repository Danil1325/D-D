using DnDGame.Domain.Common;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Talents;

/// <summary>
/// A small, curated bonus a character can pick up (from a race trait like Human's
/// "Additional Talent", or at a level-up milestone). Plugs directly into the dice
/// formula: BonusValue is added to a check whenever it uses AppliesToAttribute
/// (or to any check, if AppliesToAttribute is null).
/// </summary>
public class Talent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>The earliest character level at which this talent can be picked.</summary>
    public int MinLevel { get; set; }

    /// <summary>
    /// Which attribute's checks this talent boosts. Null means it applies to every
    /// check, not just one attribute.
    /// </summary>
    public AttributeType? AppliesToAttribute { get; set; }

    /// <summary>The flat bonus added to a matching check.</summary>
    public int BonusValue { get; set; }
}
