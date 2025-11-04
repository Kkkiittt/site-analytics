namespace Analite.Domain.Entities;

public class Block
{
	public long Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;

	public long PageId { get; set; }
	public Page Page { get; set; } = null!;

	public Guid CustomerId{ get; set; } //for faster searching block by name and apiKey
}