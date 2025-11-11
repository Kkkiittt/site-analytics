using Analite.Application.Dtos.Create;
using Analite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Analite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlocksController : Controller
{
    private readonly IBlockService _blockService;
    public BlocksController(IBlockService blockService)
    {
        _blockService = blockService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBlockAsync([FromQuery] BlockCreateDto blockCreateDto)
    {
        var block = await _blockService.CreateBlockAsync(blockCreateDto);
        return Ok(block);
    }

    [HttpPut("block_update/{id:long}")]
    public async Task<IActionResult> UpdateBlockAsync(long id, [FromBody] BlockCreateDto blockCreateDto)
    {
        var block = await _blockService.UpdateBlockAsync(id, blockCreateDto);
        return Ok(block);
    }
    
    [HttpPut("block_delete/{id:long}")]
    public async Task<IActionResult> DeleteBlockAsync(long id)
    {
        await _blockService.DeleteBlockAsync(id);
        return NoContent();
    }

    [HttpGet("block/{id:long}")]
    public async Task<IActionResult> GetByIdAsync(long id)
    {
        var block = await _blockService.GetByIdAsync(id);
        if (block == null)
            return NotFound();
        
        return Ok(block);
    }

    [HttpGet("block_paged/{pageId:long}")]
    public async Task<IActionResult> GetByPageAsync(long pageId)
    {
        var blocks = await _blockService.GetByPageAsync(pageId);
        return Ok(blocks);
    }
}