
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

namespace Analite.Application.Implementations;

public class FlowService : IFlowService
{
	private readonly AppDbContext _db;
	private readonly IDistributedCache _cache;
	private readonly IIdentityService _id;

	public FlowService(AppDbContext db, IDistributedCache cache, IIdentityService id)
	{
		_db = db;
		_cache = cache;
		_id = id;
	}

	public async Task<ManyDto<FlowGetDto>> GetFlowsAsync(Guid? id, DateTime? from, DateTime? to, PaginationData pagination)
	{
		id ??= _id.Id;
		if(id != _id.Id)
			throw new NoAccessException("Others' flows");
		int skip = (pagination.Page - 1) * pagination.PageSize;
		var query = _db.Flows.Where(f => f.CustomerId == id);
		if(from.HasValue)
		{
			query = query.Where(f => f.StartAt >= from.Value);
		}
		if(to.HasValue)
		{
			query = query.Where(f => f.EndAt <= to.Value);
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
		if(id != _id.Id)
			throw new NoAccessException("Others' flows");
		string? value = await _cache.GetStringAsync(id.ToString());
		if(value == null)
			return [];
		return (JsonSerializer.Deserialize<List<FlowGetDto>>(value) ?? []).Take(limit);
	}

	public async Task<FlowSummaryLengthDto> GetFlowSummaryByLengthAsync(Guid? customerId, DateTime? from, DateTime? to)
	{
		customerId ??= _id.Id;

		if(customerId != _id.Id)
			throw new NoAccessException("Others' flows");
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
		return new FlowSummaryDurationDto()
		{
			Minimum = min ?? throw new NotFoundException("Flows in given range"),
			Maximum = max ?? throw new NotFoundException("Flows in given range"),
			AverageDuration = TimeSpan.FromTicks((long)average),
		};
	}

	public async Task CreateFlowsAsync()
	{
		var events = _db.Events.Where(e => !e.Handled);
		var customerIds = await events.Select(e => e.CustomerId).Distinct().ToListAsync();
		foreach(var id in customerIds)
		{
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
		}
	}
}