
using Analite.Application.Dtos.Create;
using Analite.Application.Dtos.Get;
using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Domain.Exceptions;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;

namespace Analite.Application.Implementations;

public class CustomerService : ICustomerService
{
	private readonly AppDbContext _db;
	private readonly IIdentityService _id;
	private readonly ITokenService _tk;

	public CustomerService(AppDbContext db, IIdentityService id, ITokenService tk)
	{
		_db = db;
		_id = id;
		_tk = tk;
	}

	public async Task<CustomerGetDto> GetById(Guid? customerId)
	{
		customerId ??= _id.Id;
		if(customerId != _id.Id)
			throw new NoAccessException("other users");
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		return new CustomerGetDto
		{
			Id = entity.Id,
			PublicKey = entity.PublicKey,
			Name = entity.Name,
			Surname = entity.Surname,
			Email = entity.Email,
			Role = entity.Role,
			CreatedAt = entity.CreatedAt
		};
	}

	public async Task<bool> IsActiveAsync(Guid? customerId)
	{
		customerId ??= _id.Id;
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		return entity.IsActive;
	}

	public async Task<bool> IsApprovedAsync(Guid? customerId)
	{
		customerId ??= _id.Id;
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		return entity.IsApproved;
	}

	public async Task<string> LoginCustomerAsync(string email, string password)
	{
		Customer entity = await _db.Customers
			.FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower())
			?? throw new UnauthorizedException("Email or password");
		if(!BCrypt.Net.BCrypt.Verify(password, entity.PasswordHash))
			throw new UnauthorizedException("Email or password");
		if(!entity.IsApproved)
			throw new NoAccessException("Anything upon approval");
		if(!entity.IsActive)
			throw new NotFoundException("Account was deactivated");
		return _tk.GenerateToken(entity);
	}

	public async Task<string> RefreshTokenAsync(string token)
	{
		_tk.ValidateToken(token);//may throw exception
		Guid id = _tk.GetId(token);
		Customer entity = await _db.Customers.FindAsync(id) ?? throw new NotFoundException("Customer");
		string stamp = _tk.GetStamp(token);
		if(stamp != entity.SecurityStamp)
			throw new UnauthorizedException("Credentials change");
		return _tk.GenerateToken(entity);
	}

	public async Task<CustomerGetDto> RegisterCustomerAsync(CustomerCreateDto dto)
	{
		Customer entity = new Customer
		{
			Id = Guid.NewGuid(),
			PublicKey = Guid.NewGuid().ToString(),
			Name = dto.Name,
			Surname = dto.Surname,
			Email = dto.Email,
			PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
			Role = Roles.User,
			CreatedAt = DateTime.UtcNow,
			SecurityStamp = Guid.NewGuid().ToString(),
			IsActive = true,
			IsApproved = false,
			UpdatedAt = DateTime.UtcNow
		};
		_db.Customers.Add(entity);
		await _db.SaveChangesAsync();
		return new CustomerGetDto
		{
			Surname = entity.Surname,
			CreatedAt = entity.CreatedAt,
			Email = entity.Email,
			Id = entity.Id,
			Name = entity.Name,
			PublicKey = entity.PublicKey,
			Role = entity.Role
		};
	}

	public async Task ResetPublicKey(Guid? customerId)
	{
		customerId ??= _id.Id;
		if(customerId != _id.Id)
			throw new NoAccessException("reset other users key");
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		entity.PublicKey = Guid.NewGuid().ToString();
		await _db.SaveChangesAsync();
		return;
	}

	public async Task UpdateCustomerAsync(Guid? customerId, CustomerCreateDto dto)
	{
		customerId ??= _id.Id;
		if(customerId != _id.Id)
			throw new NoAccessException("update other users");
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");

		if(dto.Email != entity.Email)
		{
			bool emailExists = await _db.Customers.AnyAsync(c => c.Email.ToLower() == dto.Email.ToLower());
			if(emailExists)
				throw new UsedException("Email");
			entity.Email = dto.Email;
			entity.SecurityStamp = Guid.NewGuid().ToString();
		}

		if(!BCrypt.Net.BCrypt.Verify(dto.Password, entity.PasswordHash))
		{
			entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
			entity.SecurityStamp = Guid.NewGuid().ToString();
		}

		entity.Name = dto.Name;
		entity.Surname = dto.Surname;
		entity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync();
		return;
	}
}