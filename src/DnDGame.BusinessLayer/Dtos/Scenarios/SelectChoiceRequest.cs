namespace DnDGame.BusinessLayer.Dtos.Scenarios;

/// <summary>
/// Request shape for POST /api/scenario/choice.
/// </summary>
public class SelectChoiceRequest
{
    public int PlayerId { get; set; }
    public int SceneId { get; set; }
    public int ChoiceId { get; set; }
}