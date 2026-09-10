using DnDGame.BusinessLayer.Services.Interfaces;

namespace DnDGame.MockData.Services;

/// <summary>
/// Mock-phase ICurrentPlayerService: always resolves to a fixed id, since there is
/// no authentication yet (Task 3.24). Intentionally does not reference a Player or
/// PlayerCharacter object — see ICurrentPlayerService's remarks on why the id-only
/// approach is deliberate while the Player-vs-PlayerCharacter decision is pending.
///
/// The id below (1) is chosen to line up with the TestPlayer the team plan expects
/// MockPlayerRepository to seed once the Player entity itself is agreed on and
/// created — it is not itself a Player entity or a guess at its shape.
/// </summary>
public class MockCurrentPlayerService : ICurrentPlayerService
{
    public const int MockPlayerId = 1;

    public int GetCurrentPlayerId() => MockPlayerId;
}
