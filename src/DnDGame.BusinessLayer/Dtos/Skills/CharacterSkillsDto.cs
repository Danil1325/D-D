namespace DnDGame.BusinessLayer.Dtos.Skills;

/// <summary>
/// The skill-tree screen payload for one character: their (Race, Class) skill
/// catalog merged with their unlock state, plus their current spendable balance
/// (PlayerCharacter.SkillPoints — the same counter level-up already increments).
/// </summary>
public class CharacterSkillsDto
{
    public int PlayerId { get; init; }
    public int AvailableSkillPoints { get; init; }
    public IReadOnlyList<PlayerSkillDto> Skills { get; init; } = Array.Empty<PlayerSkillDto>();
}
