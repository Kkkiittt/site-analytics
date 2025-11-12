namespace Analite.Domain.Entities;

public class Page
{
	public long Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;

	public Guid CustomerId { get; set; }
	public Customer Customer { get; set; } = null!;

	public IList<Block> Blocks { get; set; } = new List<Block>();
}