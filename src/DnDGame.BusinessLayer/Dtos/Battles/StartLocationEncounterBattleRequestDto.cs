using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Dtos.Battles;

/// <summary>
/// Request shape for starting a battle against an enemy chosen from a location's
/// random encounter pool (ILocationEncounterService), as opposed to
/// StartBattleRequestDto's story-node-driven combat. EnemyId must currently be one
/// of ILocationEncounterService.GetAvailableEncountersAsync's results for the same
/// LocationId/GameSessionId/SubLocation — BattleService re-checks this itself
/// rather than trusting the client, reusing that service's filtering instead of
/// duplicating it.
/// </summary>
public class StartLocationEncounterBattleRequestDto
{
    public int GameSessionId { get; set; }
    public int DeckId { get; set; }
    public LocationId LocationId { get; set; }
    public int EnemyId { get; set; }
    public string? SubLocation { get; set; }
}
