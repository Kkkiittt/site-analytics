
using Analite.Application.Dtos;
using Analite.Application.Dtos.Get;
using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;

namespace Analite.Application.Implementations;

public class AdminService : IAdminService
{
	private readonly AppDbContext _db;
	private readonly IIdentityService _id;
	public AdminService(AppDbContext db, IIdentityService id)
	{
		_db = db;
		_id = id;
	}

	public async Task ApproveCustomerAsync(Guid customerId)
	{
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new InvalidOperationException("Customer not found");
		entity.IsApproved = true;
		await _db.SaveChangesAsync();
	}

	public async Task BlockCustomerAsync(Guid customerId)
	{
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new InvalidOperationException("Customer not found");
		if(entity.Role == Roles.Admin && _id.Role != Roles.SuperAdmin)
			throw new InvalidOperationException("Cannot block an admin");
		entity.IsActive = false;
		await _db.SaveChangesAsync();
	}

	public async Task<ManyDto<CustomerGetFullDto>> GetAllCustomersAsync(PaginationData pagination)
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

	public async Task<CustomerGetFullDto> GetByIdAsync(Guid customerId)
	{
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new Exception("Customer not found");
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
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new InvalidOperationException("Customer not found");
		entity.Role = Roles.Admin;
		await _db.SaveChangesAsync();
	}

	public async Task UnblockCustomerAsync(Guid customerId)
	{
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new InvalidOperationException("Customer not found");
		entity.IsActive = true;
		await _db.SaveChangesAsync();
	}
}