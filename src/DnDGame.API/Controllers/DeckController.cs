using DnDGame.BusinessLayer.Dtos.Decks;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

[ApiController]
[Route("api/deck")]
public class DeckController : ControllerBase
{
    private readonly IDeckService _deckService;

    public DeckController(IDeckService deckService)
    {
        _deckService = deckService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeckResponseDto>>> GetAll()
    {
        var result = await _deckService.GetMyDecksAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeckResponseDto>> GetById(int id)
    {
        var result = await _deckService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DeckResponseDto>> Create([FromBody] DeckSaveRequestDto request)
    {
        var result = await _deckService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DeckResponseDto>> Update(int id, [FromBody] DeckSaveRequestDto request)
    {
        var result = await _deckService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _deckService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/validate")]
    public async Task<ActionResult<DeckValidationResultDto>> Validate(int id)
    {
        var result = await _deckService.ValidateAsync(id);
        return Ok(result);
    }
}
