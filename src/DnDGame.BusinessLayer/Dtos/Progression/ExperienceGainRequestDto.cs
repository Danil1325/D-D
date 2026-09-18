namespace DnDGame.BusinessLayer.Dtos.Progression;

/// <summary>
/// Request shape for POST /api/progression/{playerId}/experience.
/// </summary>
public class ExperienceGainRequestDto
{
    /// <summary>Amount of experience to grant. Must be positive.</summary>
    public int Amount { get; set; }
}