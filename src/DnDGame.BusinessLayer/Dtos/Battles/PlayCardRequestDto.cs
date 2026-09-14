namespace DnDGame.BusinessLayer.Dtos.Battles;

/// <summary>
/// Request shape for POST /api/battle/{battleId}/play-card. No target field —
/// IBattleEngine.PlayCard(BattleContext, CardInstance?) takes none; battles are
/// single-enemy, so the target is always implicit.
/// </summary>
public class PlayCardRequestDto
{
    public Guid CardInstanceId { get; set; }
}
