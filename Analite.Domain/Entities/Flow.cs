namespace Analite.Domain.Entities;

public class Flow
{
	public long Id{ get; set; }
	public DateTime StartAt{ get; set; }
	public DateTime EndAt{ get; set; }
	public List<string> BlockNames{ get; set; } = new();
}