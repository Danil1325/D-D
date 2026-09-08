using DnDGame.Domain.Common;
using DnDGame.Domain.Entities.Talents;

namespace DnDGame.Domain.Entities.Characters;

/// <summary>
/// Records that a character picked up a given talent, and at what level — a simple
/// join between PlayerCharacter and Talent with one extra piece of information.
/// </summary>
public class CharacterTalent : BaseEntity
{
    public int CharacterId { get; set; }
    public PlayerCharacter? PlayerCharacter { get; set; }

    public int TalentId { get; set; }
    public Talent? Talent { get; set; }

    public int AcquiredAtLevel { get; set; }
}
