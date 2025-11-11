using Analite.Application.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Analite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultsController
{
    private readonly IResultService _resultService;

    public ResultsController(IResultService resultService)
    {
        _resultService = resultService;
    }
    
    [HttpGet("conversion/{customerId:guid}")]
    public async Task<IActionResult> GetConversion(Guid customerId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _resultService.GetConversion(customerId, from ?? DateTime.UtcNow.AddDays(-7), to ?? DateTime.UtcNow);
        return Ok(result);
    }
}