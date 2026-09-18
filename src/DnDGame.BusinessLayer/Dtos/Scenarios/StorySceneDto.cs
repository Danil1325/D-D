namespace DnDGame.BusinessLayer.Dtos.Scenarios;

/// <summary>
/// Response shape for GET /api/scenario/current/{playerId}.
///
/// Assembled by the scenario service from the engine's presentation copy of the
/// current scene: dialogues are returned in presentation order (English, ordered
/// by their DialogueOrder) and <see cref="Choices"/> contains only the choices
/// that are currently selectable by this player — already-taken choices and
/// choices whose requirements are unmet are excluded.
/// </summary>
public class StorySceneDto
{
    public int Id { get; init; }
    public int Act { get; init; }
    public int Chapter { get; init; }
    public string Title { get; init; } = string.Empty;
    public int LocationId { get; init; }
    public string BackgroundImage { get; init; } = string.Empty;
    public bool IsFinalScene { get; init; }

    public IReadOnlyList<DialogueDto> Dialogues { get; init; } = Array.Empty<DialogueDto>();
    public IReadOnlyList<ChoiceDto> Choices { get; init; } = Array.Empty<ChoiceDto>();
}