
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

	public async Task ApproveCustomerAsync(Guid customerId)
	{
		_log.LogInformation("Attempt to approve {CustomerId} by  {AdminId} Admin", customerId, _id.Id);

		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		entity.IsApproved = true;
		await _db.SaveChangesAsync();

		_log.LogInformation("User {CustomerId} approved", customerId);
	}

	public async Task BlockCustomerAsync(Guid customerId)
	{
		_log.LogWarning("Admin {AdminId} trying to block customer {CustomerId}", _id.Id, customerId);

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
		_log.LogWarning("HELLO WORLD!!!!!!!");
		_log.LogDebug(
			$"[{DateTime.UtcNow}] User {_id.Id} requests list of customers. Page: {pagination.Page}, PageSize: {pagination.PageSize}");

		try
		{
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
			return new ManyDto<CustomerGetFullDto>
			{
				Total = await _db.Customers.CountAsync(),
				Items = entities,
				Pagination = pagination
			};
		}
		catch(Exception ex)
		{
			_log.LogError(ex,
				"[{Time}] An error while getting customer list. User: {AdminId} ({Email}), Page: {Page}",
				DateTime.UtcNow,
				_id.Id,
				pagination.Page);
			throw;
		}

	}

	public async Task<CustomerGetFullDto> GetByIdAsync(Guid customerId)
	{
		_log.LogDebug("[{Time}] Search for a client {CustomerId} by an Admin {AdminId} ({Email})",
			DateTime.UtcNow,
			customerId,
			_id.Id);
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");

		_log.LogInformation("[{Time}] Customer {CustomerId} found by {AdminId}",
			DateTime.UtcNow,
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
		_log.LogInformation("[{Time}] User {AdminId} ({Email}) promotes customer {CustomerId} to the Admin role",
			DateTime.UtcNow,
			_id.Id);

		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		entity.Role = Roles.Admin;
		await _db.SaveChangesAsync();

		_log.LogInformation("[{Time}] User {CustomerId} successfully promoted to the Admin role by user {AdminId}",
			DateTime.UtcNow,
			customerId,
			_id.Id);
	}

	public async Task UnblockCustomerAsync(Guid customerId)
	{
		_log.LogInformation("[{Time}] Admin {AdminId} ({Email}) unblocks user {CustomerId}",
			DateTime.UtcNow,
			_id.Id,
			customerId);

		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		entity.IsActive = true;
		await _db.SaveChangesAsync();

		_log.LogInformation("[{Time}] User {CustomerId} unblocked by Admin {AdminId}",
			DateTime.UtcNow,
			customerId,
			_id.Id);
	}
}