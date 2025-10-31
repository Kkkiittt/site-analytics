namespace Analite.Application.Dtos.Create;

public class EventCreateDto
{
	public string SessionId { get; set; } = string.Empty;
	public DateTime OccuredAt { get; set; }

	public string BlockName { get; set; } = string.Empty;
	public string CustomerKey { get; set; } = string.Empty;
}