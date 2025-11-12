
using System.Text.Json;

using Analite.Application.Dtos;
using Analite.Application.Dtos.Create;
using Analite.Application.Dtos.Get;
using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Domain.Exceptions;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Analite.Application.Implementations;

public class EventService : IEventService
{
	private readonly AppDbContext _db;
	private readonly IDistributedCache _cache;

	public EventService(AppDbContext db, IDistributedCache cache)
	{
		_db = db;
		_cache = cache;
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
			OccuredAt = dto.OccuredAt,
			PageId = block.PageId,
		};
		_db.Events.Add(entity);
		await _db.SaveChangesAsync();
		await AddToCacheAsync(entity);
	}

	private async Task AddToCacheAsync(Event entity)
	{
		string key = entity.CustomerId.ToString();
		List<FlowGetDto> existing = JsonSerializer.Deserialize<List<FlowGetDto>>(await _cache.GetStringAsync(key) ?? "[]") ?? [];

		FlowGetDto? current = existing.Find(f => f.Id == entity.SessionId);
		if(current == null)
		{
			current = new FlowGetDto()
			{
				StartAt = entity.OccuredAt,
				Id = entity.SessionId,
			};
			existing.Add(current);
		}
		ShortDto block = await _db.Blocks.Where(b => b.Id == entity.BlockId).Select(b => new ShortDto()
		{
			Id = b.Id.ToString(),
			Name = b.Name,
		}).FirstOrDefaultAsync() ?? new ShortDto()
		{
			Id = "",
			Name = "Unknown",
		};
		ShortDto page = await _db.Pages.Where(p => p.Id == entity.PageId).Select(p => new ShortDto()
		{
			Id = p.Id.ToString(),
			Name = p.Name,
		}).FirstOrDefaultAsync() ?? new ShortDto()
		{
			Id = "",
			Name = "Unknown"
		};
		current.Blocks.Add(block);
		current.Pages.Add(page);
		current.EndAt = entity.OccuredAt;

		await _cache.SetStringAsync(key, JsonSerializer.Serialize(existing), new DistributedCacheEntryOptions()
		{
			SlidingExpiration = TimeSpan.FromMinutes(5)
		});
	}
}