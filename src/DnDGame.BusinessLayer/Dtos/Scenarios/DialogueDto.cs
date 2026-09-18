using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Dtos.Scenarios;

/// <summary>
/// Response shape for one line of dialogue inside a scenario scene. The domain
/// <see cref="DnDGame.Domain.Enums.DialogueType"/> enum is exposed as a string so
/// the DTO stays decoupled from the Domain assembly's enum type.
/// </summary>
public class DialogueDto
{
    public int Id { get; init; }
    public string Speaker { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public int Order { get; init; }
    public string Type { get; init; } = string.Empty;

    public static DialogueDto FromDomain(StoryDialogue dialogue) => new()
    {
        Id = dialogue.Id,
        Speaker = dialogue.Speaker,
        Text = dialogue.Text,
        Order = dialogue.DialogueOrder,
        Type = dialogue.DialogueType.ToString()
    };
}