using DnDGame.Domain.Common;

namespace DnDGame.Domain.Entities.Game;

/// <summary>A scene in the interactive scenario, containing ordered dialogue and player choices.</summary>
public class StoryScene : BaseEntity
{
    public int Act { get; set; }
    public int Chapter { get; set; }
    public string Title { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public string BackgroundImage { get; set; } = string.Empty;
    public ICollection<StoryDialogue> Dialogues { get; set; } = new List<StoryDialogue>();
    public ICollection<StoryChoice> Choices { get; set; } = new List<StoryChoice>();
    public bool IsFinalScene { get; set; }
}
