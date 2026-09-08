using DnDGame.Domain.Enums;
using DnDGame.Domain.Common;

namespace DnDGame.Domain.Entities.Races;

/// <summary>
/// The min/max range for one attribute for one race. A race has exactly 5 of these
/// rows — one per AttributeType. At character creation, the backend rolls a random
/// value in [MinValue, MaxValue] for each attribute; this is the only source of a
/// character's starting attributes, with no player-assigned standard array.
/// </summary>
public class RaceAttributeRange : BaseEntity
{
    public int RaceId { get; set; }
    public Race? Race { get; set; }

    public AttributeType Attribute { get; set; }

    /// <summary>Placeholder value — exact ranges get tuned once mock data exists (Phase 2).</summary>
    public int MinValue { get; set; }

    /// <summary>Placeholder value — exact ranges get tuned once mock data exists (Phase 2).</summary>
    public int MaxValue { get; set; }
}
