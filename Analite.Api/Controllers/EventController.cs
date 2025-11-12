using Analite.Application.Dtos.Create;
using Analite.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Analite.Api.Controllers;

[ApiController]
[Route("events")]
public class EventController : ControllerBase
{
	private readonly IEventService _serv;

	public EventController(IEventService serv)
	{
		_serv = serv;
	}

	[HttpPost]
	[AllowAnonymous]
	public async Task<IActionResult> Collect(EventCreateDto dto)
	{
		await _serv.CollectAsync(dto);
		return NoContent();
	}
}
