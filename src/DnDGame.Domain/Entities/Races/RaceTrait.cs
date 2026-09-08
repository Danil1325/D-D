using DnDGame.Domain.Common;

namespace DnDGame.Domain.Entities.Races;

/// <summary>
/// One flavor trait belonging to a Race (e.g. Orc's "Wild Angry", Elf's "Darkvision").
///
/// Design decision (Phase 1): traits are descriptive/flavor data only for now — there
/// is no mechanical "effect type" hook on this entity. Mechanical racial bonuses are
/// captured entirely by RaceAttributeRange (see Race.AttributeRanges). If a specific
/// trait needs real mechanical behavior later (e.g. poison resistance actually
/// reducing damage), that logic will live in BusinessLayer, not as a field here.
/// </summary>
public class RaceTrait : BaseEntity
{
    public int RaceId { get; set; }
    public Race? Race { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// A short key identifying which small UI icon the frontend should show next to
    /// this trait (e.g. "heart", "eye", "mask"). Not a path to a real image file —
    /// just an identifier the frontend maps to one of its own icon components.
    /// </summary>
    public string IconKey { get; set; } = string.Empty;
}
