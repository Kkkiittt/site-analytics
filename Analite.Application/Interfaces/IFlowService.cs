using Analite.Application.Dtos;
using Analite.Application.Dtos.Get;
using Analite.Application.Dtos.Results;

namespace Analite.Application.Interfaces;

public interface IFlowService
{
	Task<FlowSummaryDto> GetFlowSummaryAsync(Guid customerId, DateTime from, DateTime to, SummaryType type);

	Task<ManyDto<FlowGetDto>> GetFlowsAsync(Guid customerId, DateTime from, DateTime to, PaginationData pagination);

	Task<IEnumerable<FlowGetDto>> GetFlowsInCacheAsync(Guid customerId, int limit);
}