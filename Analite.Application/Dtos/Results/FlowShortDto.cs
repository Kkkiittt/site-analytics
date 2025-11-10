namespace Analite.Application.Dtos.Results;

public class FlowShortDto
{
	public string SessionId { get; set; } = string.Empty;
	public DateTime From { get; set; }
	public DateTime To { get; set; }
	public int PageCount { get; set; }
}