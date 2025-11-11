
using Analite.Application.Dtos.Create;
using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Domain.Exceptions;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;

namespace Analite.Application.Implementations;

public class EventService : IEventService
{
	private readonly AppDbContext _db;

	public EventService(AppDbContext db)
	{
		_db = db;
	}

	public async Task CollectAsync(EventCreateDto dto)
	{
		Customer? customer = await _db.Customers.FirstOrDefaultAsync(c => c.PublicKey == dto.CustomerKey);
		if(customer == null)
		{
			throw new NotFoundException("Customer not found");
		}

		Block? block = await _db.Blocks.FirstOrDefaultAsync(b => b.CustomerId == customer.Id && b.Name == dto.BlockName);
		if(block == null)
		{
			throw new NotFoundException("Block");
		}

		Event entity = new()
		{
			SessionId = dto.SessionId,
			BlockId = block.Id,
			CustomerId = customer.Id,
			OccuredAt = dto.OccuredAt
		};
		_db.Events.Add(entity);
		await _db.SaveChangesAsync();
	}
}