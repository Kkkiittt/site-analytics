using Analite.Application.Dtos.Results;

namespace Analite.Application.Interfaces;

public interface IResultService
{
	Task<ConversionDto> GetConversion(Guid customerId, DateTime from, DateTime to);//optional params

	Task<HeatmapDto> GetHeatmap(long pageId, DateTime from, DateTime to);//optional params
}