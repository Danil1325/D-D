namespace DnDGame.BusinessLayer.Dtos.Characters;

/// <summary>
/// Response shape for GET /api/character/options — the reference-data lists a New
/// Game screen needs to render the race and class selection cards.
/// </summary>
public class CharacterOptionsDto
{
    public IReadOnlyList<RaceOptionDto> Races { get; init; } = Array.Empty<RaceOptionDto>();
    public IReadOnlyList<ClassOptionDto> Classes { get; init; } = Array.Empty<ClassOptionDto>();
}