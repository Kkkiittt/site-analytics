using Analite.Application.Dtos;
using Analite.Application.Dtos.Results;
using Analite.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Analite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FlowsController : Controller
{
	private readonly IFlowService _flowService;

	public FlowsController(IFlowService flowService)
	{
		_flowService = flowService;
	}

	[HttpGet("{customerId:guid}/summary-length")]
	public async Task<IActionResult> GetFlowSummaryByLengthAsync(Guid customerId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
	{
		var summary = await _flowService.GetFlowSummaryByLengthAsync(customerId, from, to);
		return Ok(summary);
	}

	[HttpGet("{customerId:guid}/summary-time")]
	public async Task<IActionResult> GetFlowSummaryByDurationAsync(Guid customerId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
	{
		var summary = await _flowService.GetFlowSummaryByDurationAsync(customerId, from, to);
		return Ok(summary);
	}

	[HttpGet("{customerId:guid}")]
	public async Task<IActionResult> GetFlowsAsync(Guid customerId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
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