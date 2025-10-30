namespace Analite.Domain.Entities;

public class Flow
{
	public long Id { get; set; }
	public DateTime StartAt { get; set; }
	public DateTime EndAt { get; set; }

	public List<long> BlockIds { get; set; } = new();

	public Guid CustomerId { get; set; }
	public Customer Customer { get; set; } = null!;
}