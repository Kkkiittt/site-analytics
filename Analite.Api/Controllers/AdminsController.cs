using Analite.Application.Dtos;
using Analite.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Analite.Api.Controllers;

[ApiController]
[Route("admins")]
[Authorize(Roles ="Admin, SuperAdmin")]
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
        return Ok(customer);
    }

    [HttpGet("customers")]
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

    [HttpPut("approve/{id:guid}")]
    public async Task<IActionResult> Approve(Guid id)
    {
        await _adminService.ApproveCustomerAsync(id);
        return NoContent();
    }

    [HttpPut("block/{id:guid}")]
    public async Task<IActionResult> Block(Guid id)
    {
        await _adminService.BlockCustomerAsync(id);
        return NoContent();
    }

    [HttpPut("unblock/{id:guid}")]
    public async Task<IActionResult> Unblock(Guid id)
    {
        await _adminService.UnblockCustomerAsync(id);
        return NoContent();
    }

    [HttpPut("promote/{id:guid}")]
    public async Task<IActionResult> Promote(Guid id)
    {
        await _adminService.PromoteCustomerAsync(id);
        return NoContent();
    }
}