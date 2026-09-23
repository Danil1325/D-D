using DnDGame.BusinessLayer.Dtos.Characters;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

[ApiController]
[Route("api/character")]
public class CharacterController : ControllerBase
{
    private readonly ICharacterService _characterService;

    public CharacterController(ICharacterService characterService)
    {
        _characterService = characterService;
    }

    /// <summary>The seeded race/class options rendered by the New Game screen.</summary>
    [HttpGet("options")]
    public async Task<ActionResult<CharacterOptionsDto>> GetOptions()
    {
        var result = await _characterService.GetOptionsAsync();
        return Ok(result);
    }

    /// <summary>Creates the player's character for a New Game run.</summary>
    [HttpPost("new-game")]
    public async Task<ActionResult<CharacterResponseDto>> CreateNewGame([FromBody] NewGameCharacterRequestDto request)
    {
        var result = await _characterService.CreateNewGameAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Returns a created character by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CharacterResponseDto>> GetById(int id)
    {
        var result = await _characterService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}