using DndGame.BusinessLayer.Interfaces;
using DndGame.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DndGame.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CharactersController(ICharacterService characterService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Character>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await characterService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Character>> GetById(int id, CancellationToken cancellationToken)
    {
        var character = await characterService.GetByIdAsync(id, cancellationToken);
        return character is null ? NotFound() : Ok(character);
    }

    [HttpPost]
    public async Task<ActionResult<Character>> Create(Character character, CancellationToken cancellationToken)
    {
        var created = await characterService.CreateAsync(character, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Character character, CancellationToken cancellationToken)
    {
        if (id != character.Id)
        {
            return BadRequest("Route id must match character id.");
        }

        return await characterService.UpdateAsync(id, character, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        return await characterService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }
}
