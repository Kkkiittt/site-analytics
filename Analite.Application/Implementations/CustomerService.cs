
using Analite.Application.Dtos.Create;
using Analite.Application.Dtos.Get;
using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Domain.Exceptions;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analite.Application.Implementations;

public class CustomerService : ICustomerService
{
	private readonly AppDbContext _db;
	private readonly IIdentityService _id;
	private readonly ITokenService _tk;
	private readonly ILogger<CustomerService> _log;


	public CustomerService(AppDbContext db, IIdentityService id, ITokenService tk,  ILogger<CustomerService> log)
	{
		_db = db;
		_id = id;
		_tk = tk;
		_log = log;
	}

	public async Task<CustomerGetDto> GetById(Guid? customerId)
	{
		_log.LogDebug("Attempt to get profile for customer: {CustomerId}, Requester: {RequesterId}",
			customerId, 
			_id.Id);
		
		customerId ??= _id.Id;
		if (customerId != _id.Id)
		{
			_log.LogWarning("Unauthorized profile access attempt by {RequesterId} for customer {CustomerId}",
				_id.Id, 
				customerId);
			throw new NoAccessException("other users");
		}
		
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		
		_log.LogInformation("Profile retrieved for customer {CustomerId}", 
			entity.Id);
		
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
		
		_log.LogDebug("Checking approval status for Customer {TargetId} requested by {RequesterId}",
			customerId,
			_id.Id);
		
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		
		bool approved = entity.IsApproved;

		_log.LogInformation("Approval status for Customer {TargetId}: {IsApproved}",
			customerId,
			approved);
		
		return approved;
	}

	public async Task<string> LoginCustomerAsync(string email, string password)
	{
		_log.LogInformation("Login attempt for email: {Email}", email);

		
		Customer entity = await _db.Customers
			.FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower())
			?? throw new UnauthorizedException("Email or password");
		if (!BCrypt.Net.BCrypt.Verify(password, entity.PasswordHash))
		{
			_log.LogWarning("Invalid password for email {Email}", email);
			throw new UnauthorizedException("Email or password");
		}

		if (!entity.IsApproved)
		{
			_log.LogWarning("Login denied for {Email}: account not approved", email);
			throw new NoAccessException("Anything upon approval");
		}

		if (!entity.IsActive)
		{
			_log.LogWarning("Login denied for {Email}: account deactivated", email);
			throw new NotFoundException("Account was deactivated");
		}
			
		_log.LogInformation("Login successful for {Email}", email);
		return _tk.GenerateToken(entity);
	}

	public async Task<string> RefreshTokenAsync(string token)
	{
		_log.LogDebug("Refresh token attempt for token {TokenPart}", 
			token[..Math.Min(8, token.Length)]);
		
		_tk.ValidateToken(token);//may throw exception
		
		Guid id = _tk.GetId(token);
		Customer entity = await _db.Customers.FindAsync(id) ?? throw new NotFoundException("Customer");
		string stamp = _tk.GetStamp(token);

		if (stamp != entity.SecurityStamp)
		{
			_log.LogWarning("Token refresh denied — security stamp mismatch for Customer {CustomerId}", id);
			throw new UnauthorizedException("Credentials change");
		}
			
		_log.LogInformation("Refresh token successful for Customer {CustomerId}", id);
		return _tk.GenerateToken(entity);
	}

	public async Task<CustomerGetDto> RegisterCustomerAsync(CustomerCreateDto dto)
	{
		_log.LogInformation("Registration attempt for {Email}", dto.Email);
		
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
		
		_log.LogInformation("New user registered: {Email} (CustomerId: {CustomerId})",
			entity.Email, 
			entity.Id);
		
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
		
		_log.LogDebug("Reset public key request for Customer {CustomerId} by {RequesterId}",
			customerId,
			_id.Id);


		if (customerId != _id.Id)
		{
			_log.LogWarning("Unauthorized public key reset attempt by {RequesterId} for {CustomerId}",
				_id.Id, 
				customerId);
			throw new NoAccessException("reset other users key");
		}
			
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		entity.PublicKey = Guid.NewGuid().ToString();
		await _db.SaveChangesAsync();
		
		_log.LogInformation("Public key for Customer {CustomerId} has been updated", customerId);
	}

	public async Task UpdateCustomerAsync(Guid? customerId, CustomerCreateDto dto)
	{
		customerId ??= _id.Id;
		
		_log.LogDebug("Profile update requested by {RequesterId} for Customer {CustomerId}",
			_id.Id, customerId);

		if (customerId != _id.Id)
		{
			_log.LogWarning("Unauthorized profile update attempt by {RequesterId} for {CustomerId}",
				_id.Id,
				customerId);
			throw new NoAccessException("update other users");
		}
			
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");

		if(dto.Email != entity.Email)
		{
			bool emailExists = await _db.Customers.AnyAsync(c => c.Email.ToLower() == dto.Email.ToLower());
			if (emailExists)
			{
				_log.LogWarning("Email {Email} already used — update denied for Customer {CustomerId}",
					dto.Email, 
					customerId);
				throw new UsedException("Email");
			}
				
			entity.Email = dto.Email;
			entity.SecurityStamp = Guid.NewGuid().ToString();
		}

		if(!BCrypt.Net.BCrypt.Verify(dto.Password, entity.PasswordHash))
		{
			entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
			entity.SecurityStamp = Guid.NewGuid().ToString();
			
			_log.LogInformation("Password updated for Customer {CustomerId}", customerId);
		}

		entity.Name = dto.Name;
		entity.Surname = dto.Surname;
		entity.UpdatedAt = DateTime.UtcNow;
		await _db.SaveChangesAsync();
		
		_log.LogInformation("Profile updated for Customer {CustomerId}", customerId);
	}
}