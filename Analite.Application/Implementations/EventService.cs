
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
using Microsoft.Extensions.Logging;

namespace Analite.Application.Implementations;

public class EventService : IEventService
{
	private readonly AppDbContext _db;
	private readonly IDistributedCache _cache;
	private readonly ILogger<EventService> _log;


	public EventService(AppDbContext db, IDistributedCache cache,  ILogger<EventService> log)
	{
		_db = db;
		_cache = cache;
		_log = log;
	}

	public async Task CollectAsync(EventCreateDto dto)
	{
		_log.LogDebug("Event received: Session {SessionId}, CustomerKey {CustomerKey}, Block {BlockName}, OccuredAt {OccuredAt}",
			dto.SessionId,
			dto.CustomerKey,
			dto.BlockName,
			dto.OccuredAt);
		
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
		
		_log.LogInformation("Event stored successfully: Session {SessionId}, CustomerId {CustomerId}, Block {BlockId}, PageId {PageId}",
			entity.SessionId,
			entity.CustomerId,
			entity.BlockId,
			entity.PageId);
		
		try
		{
			await AddToCacheAsync(entity);
		}
		catch(Exception ex)
		{
			//its okay, log or sth
			//_log.LogError(ex, "Error adding event to event cache");
		}
	}

	private async Task AddToCacheAsync(Event entity)
	{
		_log.LogDebug("Adding event {SessionId} for customer {CustomerId} to cache",
			entity.SessionId,
			entity.CustomerId);
		
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

		List<ShortDto> pagesFiltered = [];
		foreach(var p in current.Pages)
		{
			if(p.Name != pagesFiltered.LastOrDefault()?.Name)
				pagesFiltered.Add(p);
		}
		current.Pages = pagesFiltered;

		await _cache.SetStringAsync(key, JsonSerializer.Serialize(existing), new DistributedCacheEntryOptions()
		{
			SlidingExpiration = TimeSpan.FromMinutes(5)
		});
		
		_log.LogInformation("Cache updated for customer {CustomerId}. Total flows cached: {FlowCount}",
			entity.CustomerId,
			existing.Count);
	}
}