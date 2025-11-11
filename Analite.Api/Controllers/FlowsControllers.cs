using Analite.Application.Dtos;
using Analite.Application.Dtos.Results;
using Analite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Analite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlowsController : Controller
{
    private readonly IFlowService _flowService;

    public FlowsController(IFlowService flowService)
    {
        _flowService = flowService;
    }
    
    [HttpGet("summary/{customerId:guid}")]
    public async Task<IActionResult> GetFlowSummary(Guid customerId, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] SummaryType type)
    {
        var summary = await _flowService.GetFlowSummaryAsync(customerId, from, to, type);
        return Ok(summary);
    }

    [HttpGet("{customerId:guid}")]
    public async Task<IActionResult> GetFlows(Guid customerId, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var pagination = new PaginationData { Page = page, PageSize = pageSize };
        var flows = await _flowService.GetFlowsAsync(customerId, from, to, pagination);
        return Ok(flows);
    }

    [HttpGet("{customerId:guid}/cache")]
    public async Task<IActionResult> GetCachedFlows(Guid customerId, [FromQuery] int limit = 10)
    {
        var cached = await _flowService.GetFlowsInCacheAsync(customerId, limit);
        return Ok(cached);
    }
}