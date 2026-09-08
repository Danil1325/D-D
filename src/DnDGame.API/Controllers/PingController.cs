using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

// A deliberately trivial controller. Its only purpose is to prove, once you run the
// project locally, that the API host, routing, and Swagger are wired up correctly —
// before any game logic exists. Feel free to delete this once RacesController and
// friends exist in Phase 4 and you have a real endpoint to check instead.
[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "ok", message = "DnDGame.API is running" });
    }
}
