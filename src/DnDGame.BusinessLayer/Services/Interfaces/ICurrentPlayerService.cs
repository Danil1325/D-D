namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Abstraction over "who is making this request" (Task 3.24), so Controllers never
/// hard-code a player id. Today this always resolves to a fixed mock id
/// (MockCurrentPlayerService); later it can be swapped for a
/// JwtCurrentPlayerService reading a claim from the auth token, with no change to
/// any Controller or Service that depends on this interface.
///
/// Deliberately returns only an int id, not a Player object: whether the
/// card-battle "Player" ends up as its own Domain entity or a set of fields added
/// to PlayerCharacter is a pending team decision (see the team plan). Keeping this
/// interface to "just an id" means it doesn't have to change, or be blocked,
/// either way that decision goes.
/// </summary>
public interface ICurrentPlayerService
{
    /// <summary>The id of the player making the current request.</summary>
    int GetCurrentPlayerId();
}
