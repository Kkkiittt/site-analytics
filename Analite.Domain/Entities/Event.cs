namespace Analite.Domain.Entities;

public class Event
{
	public string SessionId { get; set; } = string.Empty;
	public DateTime OccuredAt { get; set; }
	public string BlockName { get; set; } = string.Empty;
}