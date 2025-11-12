using Analite.Application.Dtos.Create;
using Analite.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Analite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : Controller
{
	private readonly ICustomerService _customerService;
	public CustomersController(ICustomerService customerService)
	{
		_customerService = customerService;
	}

	[HttpPost("register")]
	[AllowAnonymous]
	public async Task<IActionResult> Register([FromBody] CustomerCreateDto customerCreateDto)
	{
		var customer = await _customerService.RegisterCustomerAsync(customerCreateDto);
		return Ok(customer);
	}

	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
	{
		var login = await _customerService.LoginCustomerAsync(loginDto.Email, loginDto.Password);
		return Ok(login);
	}

	[HttpGet("refresh/{token}")]
	[AllowAnonymous]
	public async Task<IActionResult> Refresh(string token)
	{
		var result = await _customerService.RefreshTokenAsync(token);
		return Ok(result);
	}

	[HttpPut]
	public async Task<IActionResult> Update( [FromBody] CustomerCreateDto customerCreateDto, Guid? id = null)
	{
		await _customerService.UpdateCustomerAsync(id, customerCreateDto);
		return Ok();
	}

	[HttpGet]
	public async Task<IActionResult> GetById(Guid? id=null)
	{
		var customer = await _customerService.GetById(id);

		return Ok(customer);
	}

	[HttpGet("approved")]
	public async Task<IActionResult> IsApproved(Guid? id=null)
	{
		bool result = await _customerService.IsApprovedAsync(id);
		return Ok(result);
	}

	[HttpGet("active")]
	public async Task<IActionResult> IsActive(Guid? id=null)
	{
		bool result = await _customerService.IsActiveAsync(id);
		return Ok(result);
	}

	[HttpPost("reset-key")]
	public async Task<IActionResult> ResetKey(Guid? id = null)
	{
		await _customerService.ResetPublicKey(id);
		return Ok();
	}
}