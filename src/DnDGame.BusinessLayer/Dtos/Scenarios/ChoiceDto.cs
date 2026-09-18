using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Dtos.Scenarios;

/// <summary>
/// Response shape for one selectable choice. The service only ever maps choices
/// that are currently available to the player (not already selected and with all
/// requirements met), so this DTO carries no requirement information — the client
/// must not be shown what the player cannot choose.
/// </summary>
public class ChoiceDto
{
    public int Id { get; init; }
    public string Text { get; init; } = string.Empty;

    /// <summary>Null when choosing this entry ends the scenario.</summary>
    public int? NextSceneId { get; init; }

    public static ChoiceDto FromDomain(StoryChoice choice) => new()
    {
        Id = choice.Id,
        Text = choice.Text,
        NextSceneId = choice.NextSceneId
    };
}