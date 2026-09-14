namespace DnDGame.BusinessLayer.Dtos.Battles;

/// <summary>
/// Request shape for POST /api/battle/start. The enemy is not chosen by the
/// client — it's resolved from the game session's current story node
/// (StoryNode.EnemyId / Choice.EnemyId), same as the adventure/story system
/// already models combat encounters.
/// </summary>
public class StartBattleRequestDto
{
    public int GameSessionId { get; set; }
    public int DeckId { get; set; }
}
