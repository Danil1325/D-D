using DnDGame.Domain.Common;

namespace DnDGame.Domain.Entities.Game;

public class StoryChoice : BaseEntity
{
    public string Text { get; set; } = string.Empty;

    /// <summary>Null when the choice ends the scenario without another scene.</summary>
    public int? NextSceneId { get; set; }

    /// <summary>All requirements must be satisfied. An empty collection means unrestricted access.</summary>
    public ICollection<ChoiceRequirement> Requirements { get; set; } = new List<ChoiceRequirement>();

    public ICollection<ChoiceConsequence> Consequences { get; set; } = new List<ChoiceConsequence>();
}
