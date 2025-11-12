namespace Analite.Domain.Entities;

public class Event
{
	public long Id { get; set; }
	public string SessionId { get; set; } = string.Empty;
	public DateTime OccuredAt { get; set; }

	public long BlockId { get; set; }
	public long PageId { get; set; }
	public Guid CustomerId { get; set; }
}