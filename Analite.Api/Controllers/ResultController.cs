using Analite.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Analite.Api.Controllers;

[ApiController]
[Route("results")]
[Authorize]
public class ResultsController : ControllerBase
{
	private readonly IResultService _resultService;

	public ResultsController(IResultService resultService)
	{
		_resultService = resultService;
	}

	[HttpGet("conversion")]
	public async Task<IActionResult> GetConversionAsync(Guid? customerId = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
	{
		return Ok(await _resultService.GetConversionAsync(customerId, from, to));
	}

	[HttpGet("heatmap/{pageId:long}")]
	public async Task<IActionResult> GetHeatmapAsync(long pageId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
	{
		return Ok(await _resultService.GetHeatmapAsync(pageId, from, to));
	}
}