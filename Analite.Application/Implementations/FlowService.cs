
using System.Text.Json;

using Analite.Application.Dtos;
using Analite.Application.Dtos.Get;
using Analite.Application.Dtos.Results;
using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Domain.Exceptions;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Analite.Application.Implementations;

public class FlowService : IFlowService
{
	private readonly AppDbContext _db;
	private readonly IDistributedCache _cache;
	private readonly IIdentityService _id;
	private readonly ILogger<FlowService> _log;


	public FlowService(AppDbContext db, IDistributedCache cache, IIdentityService id,  ILogger<FlowService> log)
	{
		_db = db;
		_cache = cache;
		_id = id;
		_log = log;
	}

	public async Task<ManyDto<FlowGetDto>> GetFlowsAsync(Guid? id, DateTime? from, DateTime? to, PaginationData pagination)
	{
		id ??= _id.Id;
		if (id != _id.Id)
		{
			_log.LogWarning("Unauthorized flows access attempt by {UserId} for CustomerId {TargetId}",
				_id.Id, 
				id);
			throw new NoAccessException("Others' flows");
		}
		_log.LogDebug(
			"[{Time}] Fetching flows for CustomerId {CustomerId}, Page {Page}, Size {Size}, From {From}, To {To}",
			DateTime.UtcNow, 
			id,
			pagination.Page, 
			pagination.PageSize, 
			from, 
			to
		);
			
		int skip = (pagination.Page - 1) * pagination.PageSize;
		var query = _db.Flows.Where(f => f.CustomerId == id);
		if(from.HasValue)
		{
			query = query.Where(f => f.StartAt >= from.Value);
			_log.LogDebug("Applied filter: StartAt >= {From}", from);
		}
		if(to.HasValue)
		{
			query = query.Where(f => f.EndAt <= to.Value);
			_log.LogDebug("Applied filter: EndAt <= {To}", to);
		}
		var total = await query.CountAsync();
		var res = await query.OrderByDescending(f => f.StartAt)
			.Skip(skip)
			.Take(pagination.PageSize)
			.Select(f =>

				new
				{
					StartAt = f.StartAt,
					Blocks = f.BlockIds,
					Pages = f.PageIds,
					EndAt = f.EndAt,
					Id = f.SessionId.ToString(),
				}
			).ToListAsync();
		var blockIds = res.SelectMany(r => r.Blocks).Distinct().ToList();
		var blocks = await _db.Blocks
			.Where(b => blockIds.Contains(b.Id))
			.Select(b => new
			{
				b.Id,
				b.Name,
				b.PageId,
				PageName = b.Page.Name
			})
			.ToDictionaryAsync(b => b.Id, b => b);
		var pageIds = res.SelectMany(r => r.Pages).Distinct().ToList();
		var pages = await _db.Pages
			.Where(p => pageIds.Contains(p.Id))
			.ToDictionaryAsync(p => p.Id, p => p.Name);
		var items = res.Select(r => new FlowGetDto()
		{
			StartAt = r.StartAt,
			EndAt = r.EndAt,
			Id = r.Id,
			Blocks = r.Blocks.Select(bid => new ShortDto()
			{
				Id = bid.ToString(),
				Name = blocks.ContainsKey(bid) ? blocks[bid].Name : "Unknown",
			}).ToList(),
			Pages = r.Pages.Select(pid => new ShortDto()
			{
				Id = pid.ToString(),
				Name = pages.ContainsKey(pid) ? pages[pid] : "Unknown",
			}).ToList()
			,
		}).ToList();
		
		_log.LogInformation(
			"Flows retrieved for CustomerId {CustomerId}: {Count} items",
			id,
			items.Count);
		
		return new ManyDto<FlowGetDto>()
		{
			Total = total,
			Items = items,
			Pagination = pagination,
		};
	}

	public async Task<IEnumerable<FlowGetDto>> GetFlowsInCacheAsync(Guid? id, int limit)
	{
		id ??= _id.Id;
		if (id != _id.Id)
		{
			_log.LogWarning("Unauthorized cached flows access by {UserId} for CustomerId {TargetId}",
				_id.Id,
				id);
			throw new NoAccessException("Others' flows");
		}
		
		_log.LogDebug("[{Time}] Fetching last {Limit} cached flows for CustomerId {CustomerId}",
			DateTime.UtcNow, 
			limit,
			id);
			
		string? value = await _cache.GetStringAsync(id.ToString());
		if (value == null)
		{
			_log.LogDebug("No cached flows for {CustomerId}", id);
			return [];
		}
			
		var list = JsonSerializer.Deserialize<List<FlowGetDto>>(value) ?? new();
		_log.LogInformation("Cached flows for CustomerId {CustomerId}: {Count}", id, list.Count);
		return (JsonSerializer.Deserialize<List<FlowGetDto>>(value) ?? []).Take(limit);
	}

	public async Task<FlowSummaryLengthDto> GetFlowSummaryByLengthAsync(Guid? customerId, DateTime? from, DateTime? to)
	{
		customerId ??= _id.Id;

		if(customerId != _id.Id)
			throw new NoAccessException("Others' flows");
		
		_log.LogDebug("Calculating flow length summary for {CustomerId}", customerId);
		
		var query = _db.Flows.Where(f => f.CustomerId == customerId);
		if(from.HasValue)
		{
			query = query.Where(f => f.StartAt >= from.Value);
		}
		if(to.HasValue)
		{
			query = query.Where(f => f.EndAt <= to.Value);
		}
		var min = await query.OrderBy(f => f.PageIds.Count).Select(f => new FlowShortDto()
		{
			SessionId = f.SessionId.ToString(),
			PageCount = f.PageIds.Count,
			From = f.StartAt,
			To = f.EndAt,
		}).FirstOrDefaultAsync();
		var max = await query.OrderByDescending(f => f.PageIds.Count).Select(f => new FlowShortDto()
		{
			SessionId = f.SessionId.ToString(),
			PageCount = f.PageIds.Count,
			From = f.StartAt,
			To = f.EndAt,
		}).FirstOrDefaultAsync();
		
		_log.LogInformation("Flow length summary calculated for {CustomerId}", customerId);
		
		var average = await query.AverageAsync(f => f.PageIds.Count);
		return new FlowSummaryLengthDto()
		{
			Minimum = min ?? throw new NotFoundException("Flows in given range"),
			Maximum = max ?? throw new NotFoundException("Flows in given range"),
			AverageLength = (float)average,
		};
	}

	public async Task<FlowSummaryDurationDto> GetFlowSummaryByDurationAsync(Guid? customerId, DateTime? from, DateTime? to)
	{
		customerId ??= _id.Id;
		if(customerId != _id.Id)
			throw new NoAccessException("Others' flows");
		
		_log.LogDebug("Calculating flow duration summary for {CustomerId}", customerId);
		
		var query = _db.Flows.Where(f => f.CustomerId == customerId);
		if(from.HasValue)
		{
			query = query.Where(f => f.StartAt >= from.Value);
		}
		if(to.HasValue)
		{
			query = query.Where(f => f.EndAt <= to.Value);
		}
		var min = await query.OrderBy(f => f.EndAt - f.StartAt).Select(f => new FlowShortDto()
		{
			SessionId = f.SessionId.ToString(),
			PageCount = f.PageIds.Count,
			From = f.StartAt,
			To = f.EndAt,
		}).FirstOrDefaultAsync();
		var max = await query.OrderByDescending(f => f.EndAt - f.StartAt).Select(f => new FlowShortDto()
		{
			SessionId = f.SessionId.ToString(),
			PageCount = f.PageIds.Count,
			From = f.StartAt,
			To = f.EndAt,
		}).FirstOrDefaultAsync();
		var average = await query.AverageAsync(f => (f.EndAt - f.StartAt).Ticks);
		
		_log.LogInformation("Flow duration summary calculated for {CustomerId}", customerId);

		return new FlowSummaryDurationDto()
		{
			Minimum = min ?? throw new NotFoundException("Flows in given range"),
			Maximum = max ?? throw new NotFoundException("Flows in given range"),
			AverageDuration = TimeSpan.FromTicks((long)average),
		};
	}

	public async Task CreateFlowsAsync()
	{
		_log.LogInformation("Creating flows from events");

		var events = _db.Events.Where(e => !e.Handled);
		var customerIds = await events.Select(e => e.CustomerId).Distinct().ToListAsync();
		foreach(var id in customerIds)
		{
			_log.LogDebug("Processing flows for CustomerId {CustomerId}", id);

			var groups = await events.Where(e => e.CustomerId == id)
				.GroupBy(e => e.SessionId).ToListAsync();
			var flows = groups.Select(g => new Flow()
			{
				SessionId = g.Key,
				StartAt = g.Min(e => e.OccuredAt),
				EndAt = g.Max(e => e.OccuredAt),
				BlockIds = g.Select(e => e.BlockId).ToList(),
				CustomerId = id,
				PageIds = g.Select(e => e.PageId).ToList()
			}).ToList();
			foreach(var flow in flows)
			{
				var pagesFiltered = new List<long>();
				foreach(var pageId in flow.PageIds)
				{
					if(pageId != pagesFiltered.LastOrDefault())
						pagesFiltered.Add(pageId);
				}
				flow.PageIds = pagesFiltered;
			}

			_db.Flows.AddRange(flows);
			await events.ExecuteUpdateAsync(st =>
				st.SetProperty(e => e.Handled, true)
			);
			await _db.SaveChangesAsync();
			
			_log.LogInformation("Flows created for CustomerId {CustomerId}: {Count}", id, flows.Count);

		}
	}
}