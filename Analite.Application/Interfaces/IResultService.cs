using Analite.Application.Dtos.Get;

namespace Analite.Application.Interfaces;

public interface IResultService
{
	Task<ConversionGetDto> GetConversion(Guid customerId, DateTime from, DateTime to);//optional params

	Task<HeatmapGetDto> GetHeatmap(long pageId, DateTime from, DateTime to);//optional params
}