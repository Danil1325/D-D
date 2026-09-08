using DnDGame.Domain.Common;
using DnDGame.Domain.Entities.Classes;
using DnDGame.Domain.Entities.Races;

namespace DnDGame.Domain.Entities.Portraits;

/// <summary>
/// The finished character-art image for one specific (Race, Class) combination
/// (e.g. "orc_healer.png"). All 16 combinations exist as real image files and are
/// seeded as 16 rows of this table. Looked up by (RaceId, ClassId) rather than
/// computed from a naming convention, so a missing or renamed image is a data fix,
/// not a code fix.
/// </summary>
public class CharacterPortrait : BaseEntity
{
    public int RaceId { get; set; }
    public Race? Race { get; set; }

    public int ClassId { get; set; }
    public CharacterClass? CharacterClass { get; set; }

    public string ImagePath { get; set; } = string.Empty;
}
