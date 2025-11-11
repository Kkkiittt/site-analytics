using Analite.Application.Dtos;
using Analite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Analite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminsController : Controller
{
    private readonly IAdminService _adminService;
    public AdminsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("customers/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _adminService.GetByIdAsync(id);
        if (customer == null) 
            return NotFound();
        
        return Ok(customer);
    }

    [HttpGet("customers_getAll")]
    public async Task<IActionResult> GetAllCustomers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var pagination = new PaginationData
        {
            Page = page,
            PageSize = pageSize,
        };
        var pagedResult = await _adminService.GetAllCustomersAsync(pagination);
        return Ok(pagedResult);
    }

    [HttpPut("customers_approve/{id:guid}")]
    public async Task<IActionResult> Approve(Guid id)
    {
        await _adminService.ApproveCustomerAsync(id);
        return Ok();
    }

    [HttpPut("customers_block/{id:guid}")]
    public async Task<IActionResult> Block(Guid id)
    {
        await _adminService.BlockCustomerAsync(id);
        return Ok();
    }

    [HttpPut("customers_unblock/{id:guid}")]
    public async Task<IActionResult> Unblock(Guid id)
    {
        await _adminService.UnblockCustomerAsync(id);
        return Ok();
    }

    [HttpPut("customers_promote/{id:guid}")]
    public async Task<IActionResult> Promote(Guid id)
    {
        await _adminService.PromoteCustomerAsync(id);
        return Ok();
    }
}