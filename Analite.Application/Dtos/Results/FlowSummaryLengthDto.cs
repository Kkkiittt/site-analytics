namespace Analite.Application.Dtos.Results;

public class FlowSummaryLengthDto
{
	public FlowShortDto Minimum { get; set; } = new FlowShortDto();
	public FlowShortDto Maximum { get; set; } = new FlowShortDto();
	public float AverageLength { get; set; }
}