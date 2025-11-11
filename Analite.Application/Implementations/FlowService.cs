
using Analite.Application.Dtos;
using Analite.Application.Dtos.Get;
using Analite.Application.Dtos.Results;
using Analite.Application.Interfaces;
using Analite.Infrastructure.EFCore;

using Microsoft.EntityFrameworkCore;

namespace Analite.Application.Implementations;

public class FlowService : IFlowService
{
	private readonly AppDbContext _db;

	public FlowService(AppDbContext db)
	{
		_db = db;
	}

	public async Task<ManyDto<FlowGetDto>> GetFlowsAsync(Guid customerId, DateTime? from, DateTime? to, PaginationData pagination)
	{
		int skip = (pagination.Page - 1) * pagination.PageSize;
		var query = _db.Flows.Where(f => f.CustomerId == customerId);
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
		var pages = blocks.Values
			.GroupBy(b => b.PageId)
			.ToDictionary(g => g.Key, g => g.First().PageName);
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
			Pages = r.Blocks.Where(bid => blocks.ContainsKey(bid))
			.Select(bid => blocks[bid].PageId)
			.Distinct()
			.Select(pid => new ShortDto()
			{
				Id = pid.ToString(),
				Name = pages.ContainsKey(pid) ? pages[pid] : "Unknown",
			}).ToList()
			,
		}).ToList();
	}

	public Task<IEnumerable<FlowGetDto>> GetFlowsInCacheAsync(Guid customerId, int limit)
	{
		throw new NotImplementedException();
	}

	public Task<FlowSummaryDto> GetFlowSummaryAsync(Guid customerId, DateTime? from, DateTime? to, SummaryType type)
	{
		throw new NotImplementedException();
	}
}