using DnDGame.Domain.Common;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Game;

public class StoryDialogue : BaseEntity
{
    public string Speaker { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    /// <summary>Dialogue is presented in ascending order of this value.</summary>
    public int DialogueOrder { get; set; }

    public DialogueType DialogueType { get; set; } = DialogueType.Narration;
}
