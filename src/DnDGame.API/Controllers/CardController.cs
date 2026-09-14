using DnDGame.BusinessLayer.Dtos.Cards;
using DnDGame.BusinessLayer.Dtos.Common;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

[ApiController]
[Route("api/card")]
public class CardController : ControllerBase
{
    private readonly ICardService _cardService;

    public CardController(ICardService cardService)
    {
        _cardService = cardService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CardDetailsDto>>> Search([FromQuery] CardSearchRequestDto request)
    {
        var result = await _cardService.SearchCardsAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CardDetailsDto>> GetById(int id)
    {
        var result = await _cardService.GetCardByIdAsync(id);
        return Ok(result);
    }
}
