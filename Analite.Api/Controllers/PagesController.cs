using Analite.Application.Dtos.Create;
using Analite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Analite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagesController : Controller
{
    private readonly IPageService _pageService;

    public PagesController(IPageService pageService)
    {
        _pageService = pageService;
    }
    
    [HttpPost("page_create")]
    public async Task<IActionResult> Create([FromBody] PageCreateDto pageCreateDto)
    {
        var page = await _pageService.CreatePageAsync(pageCreateDto);
        return Ok(page);
    }

    [HttpPut("page_update/{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] PageCreateDto pageCreateDto)
    {
        var result = await _pageService.UpdatePageAsync(id, pageCreateDto);
        return Ok(result);
    }

    [HttpDelete("page_delete/{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _pageService.DeletePageAsync(id);
        return NoContent();
    }

    [HttpGet("page_byId{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var page = await _pageService.GetByIdAsync(id);
        if (page == null)
            return NotFound();

        return Ok(page);
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        var pages = await _pageService.GetByCustomerAsync(customerId);
        return Ok(pages);
    }

    [HttpGet("{pageId:long}/visits")]
    public async Task<IActionResult> GetVisits(long pageId)
    {
        var count = await _pageService.GetVisitsCountsAsync(pageId);
        return Ok(new { PageId = pageId, Visits = count });
    }

    [HttpGet("{pageId:long}/unique_users")]
    public async Task<IActionResult> GetUniqueUsers(long pageId)
    {
        var count = await _pageService.GetUniqueUsersCountsAsync(pageId);
        return Ok(new { PageId = pageId, UniqueUsers = count });
    }
}