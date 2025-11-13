
using Analite.Application.Dtos;
using Analite.Application.Dtos.Get;
using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Domain.Exceptions;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analite.Application.Implementations;

public class AdminService : IAdminService
{
	private readonly AppDbContext _db;
	private readonly IIdentityService _id;
	private readonly ILogger<AdminService> _log;
	

	public AdminService(AppDbContext db, IIdentityService id, ILogger<AdminService> log)
	{
		_db = db;
		_id = id;
		_log = log;
	}
	
	private void EnsureAdmin()
	{
		if (_id.Role != Roles.Admin && _id.Role != Roles.SuperAdmin)
		{
			_log.LogWarning("Access denied. User {UserId} with role {Role} attempted admin action.", 
				_id.Id, 
				_id.Role);
			throw new NoAccessException("Admin permission required");
		}
	}

	public async Task ApproveCustomerAsync(Guid customerId)
	{
		EnsureAdmin();
		_log.LogInformation("Admin {AdminId} attempts to approve customer {CustomerId}",
			_id.Id, 
			customerId);
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		entity.IsApproved = true;
		await _db.SaveChangesAsync();

		_log.LogInformation("User {CustomerId} approved", customerId);
	}

	public async Task BlockCustomerAsync(Guid customerId)
	{
		EnsureAdmin();
		_log.LogWarning("Admin {AdminId} trying to block customer {CustomerId}",
			_id.Id, 
			customerId);

		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		if(entity.Role == Roles.Admin && _id.Role != Roles.SuperAdmin)
		{
			_log.LogWarning("Attempt to block an administrator without SuperAdmin rights. AdminId: {AdminId}", _id.Id);
			throw new InvalidOperationException("Cannot block an admin");
		}

		entity.IsActive = false;
		await _db.SaveChangesAsync();

		_log.LogInformation("User {CustomerId} blocked by Admin {AdminId}", customerId, _id.Id);
	}

	public async Task<ManyDto<CustomerGetFullDto>> GetAllCustomersAsync(PaginationData pagination)
	{
		EnsureAdmin();
		_log.LogDebug("User {AdminId}  requests list of customers. Page: {Page}, PageSize: {PageSize}",
			_id.Id,
			pagination.Page,
			pagination.PageSize);

		var entities = await _db.Customers.OrderByDescending(c => c.CreatedAt)
			.Skip((pagination.Page - 1) * pagination.PageSize)
			.Take(pagination.PageSize)
			.Select(c => new CustomerGetFullDto
			{
				Id = c.Id,
				PublicKey = c.PublicKey,
				Name = c.Name,
				Surname = c.Surname,
				Email = c.Email,
				Role = c.Role,
				CreatedAt = c.CreatedAt,
				SecurityStamp = c.SecurityStamp,
				IsActive = c.IsActive,
				IsApproved = c.IsApproved,
				UpdatedAt = c.UpdatedAt
			})
			.ToListAsync();
		
		_log.LogInformation("Admin {AdminId} retrieved customer list: {Count} items",
			_id.Id, 
			entities.Count);
		
		return new ManyDto<CustomerGetFullDto>
		{
			Total = await _db.Customers.CountAsync(),
			Items = entities,
			Pagination = pagination
		};

	}

	public async Task<CustomerGetFullDto> GetByIdAsync(Guid customerId)
	{
		EnsureAdmin();
		_log.LogDebug("Search for a client {CustomerId} by an Admin {AdminId})",
			customerId,
			_id.Id);
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");

		_log.LogInformation("Customer {CustomerId} found by {AdminId}",
			customerId,
			_id.Id);

		return new CustomerGetFullDto
		{
			Id = entity.Id,
			PublicKey = entity.PublicKey,
			Name = entity.Name,
			Surname = entity.Surname,
			Email = entity.Email,
			Role = entity.Role,
			CreatedAt = entity.CreatedAt,
			IsActive = entity.IsActive,
			SecurityStamp = entity.SecurityStamp,
			IsApproved = entity.IsApproved,
			UpdatedAt = entity.UpdatedAt
		};
	}

	public async Task PromoteCustomerAsync(Guid customerId)
	{
		EnsureAdmin();
		_log.LogInformation("User {AdminId}  promotes customer {CustomerId} to the Admin role",
			_id.Id,
			customerId);

		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		entity.Role = Roles.Admin;
		await _db.SaveChangesAsync();

		_log.LogInformation("User {CustomerId} successfully promoted to the Admin role by user {AdminId}",
			customerId,
			_id.Id);
	}

	public async Task UnblockCustomerAsync(Guid customerId)
	{
		EnsureAdmin();
		_log.LogInformation("Admin {AdminId} unblocks user {CustomerId}",
			_id.Id,
			customerId);

		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		entity.IsActive = true;
		await _db.SaveChangesAsync();

		_log.LogInformation("User {CustomerId} unblocked by Admin {AdminId}",
			customerId,
			_id.Id);
	}
}