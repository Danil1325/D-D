using DnDGame.BusinessLayer.Dtos.Dice;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

[ApiController]
[Route("api/dice")]
public class DiceController : ControllerBase
{
    private readonly IDiceService _diceService;

    public DiceController(IDiceService diceService)
    {
        _diceService = diceService;
    }

    [HttpPost("roll")]
    public ActionResult<DiceResultDto> Roll([FromBody] DiceRequestDto request)
    {
        var result = _diceService.Roll(request);
        return Ok(result);
    }
}
