using Analite.Application.Dtos.Create;
using Analite.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Analite.Api.Controllers;

[ApiController]
[Route("pages")]
[Authorize]
public class PagesController : Controller
{
    private readonly IPageService _pageService;

    public PagesController(IPageService pageService)
    {
        _pageService = pageService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PageCreateDto pageCreateDto)
    {
        var page = await _pageService.CreatePageAsync(pageCreateDto);
        return Ok(page);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] PageCreateDto pageCreateDto)
    {
        var result = await _pageService.UpdatePageAsync(id, pageCreateDto);
        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _pageService.DeletePageAsync(id);
        return NoContent();
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var page = await _pageService.GetByIdAsync(id);

        return Ok(page);
    }

    [HttpGet("customer")]
    public async Task<IActionResult> GetByCustomer(Guid? customerId = null)
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

    [HttpGet("{pageId:long}/users-unique")]
    public async Task<IActionResult> GetUniqueUsers(long pageId)
    {
        var count = await _pageService.GetUniqueUsersCountsAsync(pageId);
        return Ok(new { PageId = pageId, UniqueUsers = count });
    }
}