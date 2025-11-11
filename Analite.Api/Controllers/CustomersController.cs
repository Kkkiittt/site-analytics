using Analite.Application.Dtos.Create;
using Analite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Analite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;
    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CustomerCreateDto customerCreateDto)
    {
        var customer = await _customerService.RegisterCustomerAsync(customerCreateDto);
        return Ok(customer);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var login = await _customerService.LoginCustomerAsync(loginDto.Email, loginDto.Password);
        if  (login == null)
            return BadRequest();
        return Ok(login);
    }
    
    [HttpPut("customer_update/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CustomerCreateDto customerCreateDto)
    {
        await _customerService.UpdateCustomerAsync(id, customerCreateDto);
        return Ok();
    }
    
    [HttpGet("customer_getById/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _customerService.GetById(id);
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }
    
    [HttpGet("{id:guid}/approved")]
    public async Task<IActionResult> IsApproved(Guid id)
    {
        bool result = await _customerService.IsApprovedAsync(id);
        return Ok(result);
    }

    [HttpGet("{id:guid}/active")]
    public async Task<IActionResult> IsActive(Guid id)
    {
        bool result = await _customerService.IsActiveAsync(id);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reset-key")]
    public async Task<IActionResult> ResetKey(Guid id)
    {
        await _customerService.ResetPublicKey(id);
        return Ok();
    }
}