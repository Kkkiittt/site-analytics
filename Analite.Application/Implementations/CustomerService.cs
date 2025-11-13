
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


	public CustomerService(AppDbContext db, IIdentityService id, ITokenService tk, ILogger<CustomerService> log)
	{
		_db = db;
		_id = id;
		_tk = tk;
		_log = log;
	}

	public async Task<CustomerGetDto> GetById(Guid? customerId)
	{
		_log.LogDebug("[{Time}] Attempt to get profile for customer: {CustomerId}, Requester: {RequesterId}",
			DateTime.UtcNow, 
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

	public async Task<bool> IsActiveAsync(Guid customerId)
	{
		_log.LogDebug("Checking IsActive for customer {CustomerId}, Requester: {RequesterId}", 
			customerId, 
			_id.Id);


		if (customerId != _id.Id)
		{
			_log.LogWarning("Unauthorized IsActive check by {RequesterId} for CustomerId {CustomerId}", 
				_id.Id, 
				customerId);
			throw new NoAccessException("others activeness");
		}
			
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		
		_log.LogInformation("Customer {CustomerId} active status: {IsActive}", 
			entity.Id, 
			entity.IsActive);
		
		return entity.IsActive;
	}

	public async Task<bool> IsApprovedAsync(Guid customerId)
	{
		_log.LogDebug("Checking IsApproved for customer {CustomerId}, Requester: {RequesterId}", 
			customerId, 
			_id.Id);


		if (customerId != _id.Id)
		{
			_log.LogWarning("Unauthorized IsApproved check by {RequesterId} for customer {CustomerId}", 
				_id.Id, 
				customerId);
			throw new NoAccessException("others approval");
		}
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		
		_log.LogInformation("Customer {CustomerId} approval status: {IsApproved}", 
			entity.Id, 
			entity.IsApproved);
		
		return entity.IsApproved;
	}

	public async Task<string> LoginCustomerAsync(string email, string password)
	{
		_log.LogInformation("Login attempt for email: {Email}", email);

		try
		{
			Customer entity = await _db.Customers
				                  .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower())
			                  ?? throw new UnauthorizedException("Email or password");
			if (!BCrypt.Net.BCrypt.Verify(password, entity.PasswordHash))
			{
				_log.LogWarning("Invalid password for email {Email}", email);
				throw new UnauthorizedException("Email or password");
			}

			_log.LogInformation("Login successful for {Email}", email);
			return _tk.GenerateToken(entity);
		}
		catch(Exception ex)
		{
			_log.LogError(ex, "Error while logging in user {Email}", email);
			throw;
		}
		
	}

	public async Task<string> RefreshTokenAsync(string token)
	{
		_log.LogDebug("Refresh token attempt for token {TokenSnippet}", token[..Math.Min(8, token.Length)]);

		try
		{
			_tk.ValidateToken(token); //may throw exception
			Guid id = _tk.GetId(token);
			Customer entity = await _db.Customers.FindAsync(id) ?? throw new NotFoundException("Customer");
			string stamp = _tk.GetStamp(token);
			if (stamp != entity.SecurityStamp)
			{
				_log.LogWarning("Invalid refresh attempt — security stamp mismatch for customer {CustomerId}", id);
				throw new UnauthorizedException("Credentials change");
			}
				
			_log.LogInformation("Token refreshed successfully for customer {CustomerId}", id);
			return _tk.GenerateToken(entity);
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error while refreshing token");
			throw;
		}
		
		
	}

	public async Task<CustomerGetDto> RegisterCustomerAsync(CustomerCreateDto dto)
	{
		_log.LogInformation("Registration attempt for email: {Email}", dto.Email);

		try
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
			
			_log.LogInformation("New user registered: {Email}, CustomerId: {CustomerId}", 
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
		catch (Exception ex)
		{
			_log.LogError(ex, "Registration error for email {Email}", dto.Email);
			throw;
		}
		
	}

	public async Task ResetPublicKey(Guid customerId)
	{
		_log.LogDebug("Reset public key attempt for customer {CustomerId} by {RequesterId}", 
			customerId, 
			_id.Id);

		if (customerId != _id.Id)
		{
			_log.LogWarning("Unauthorized key reset attempt by {RequesterId} for customer {CustomerId}", 
				_id.Id,
				customerId);
			throw new NoAccessException("reset other users key");
		}
			
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");
		entity.PublicKey = Guid.NewGuid().ToString();
		await _db.SaveChangesAsync();
		
		_log.LogInformation("Public key reset for customer {CustomerId}", customerId);
		
	}

	public async Task UpdateCustomerAsync(Guid customerId, CustomerCreateDto dto)
	{
		_log.LogDebug("Updating profile for customer {CustomerId} by {RequesterId}", 
			customerId, 
			_id.Id);


		if (customerId != _id.Id)
		{
			_log.LogWarning("Unauthorized update attempt by {RequesterId} for customer {CustomerId}", 
				_id.Id,
				customerId);
			throw new NoAccessException("update other users");
		}
			
		Customer entity = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Customer");

		try
		{
			if (dto.Email != entity.Email)
			{
				bool emailExists = await _db.Customers.AnyAsync(c => c.Email.ToLower() == dto.Email.ToLower());
				if (emailExists)
				{
					_log.LogWarning("Attempt to use existing email {Email} by customer {CustomerId}", 
						dto.Email, 
						customerId);
					throw new UsedException("Email");
				}
					
				entity.Email = dto.Email;
				entity.SecurityStamp = Guid.NewGuid().ToString();
			}

			if (!BCrypt.Net.BCrypt.Verify(dto.Password, entity.PasswordHash))
			{
				entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
				entity.SecurityStamp = Guid.NewGuid().ToString();
				_log.LogInformation("Password changed for customer {CustomerId}", customerId);
			}

			entity.Name = dto.Name;
			entity.Surname = dto.Surname;
			entity.UpdatedAt = DateTime.UtcNow;
			await _db.SaveChangesAsync();
			
			_log.LogInformation("Profile updated successfully for customer {CustomerId}", customerId);
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Error updating profile for customer {CustomerId}", customerId);
			throw;
		}
		
	}
}