using DnDGame.BusinessLayer.Dtos.Collection;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

/// <summary>
/// Collection endpoint. Read-only and not scoped to any player — see
/// ICollectionService for why there is no per-player "discovered" state today.
/// </summary>
[ApiController]
[Route("api/collection")]
public class CollectionController : ControllerBase
{
    private readonly ICollectionService _collectionService;

    public CollectionController(ICollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    /// <summary>The whole Collection catalog.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CollectionEntryDto>>> GetCatalog()
    {
        var result = await _collectionService.GetCatalogAsync();
        return Ok(result);
    }
}
