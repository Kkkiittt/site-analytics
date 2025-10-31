namespace Analite.Application.Dtos;

public class ManyDto<T>
{
	public int Total { get; set; }
	public List<T> Items { get; set; } = new();

	public PaginationData? Pagination { get; set; } = new();
}

public class PaginationData
{
	public int Page { get; set; }
	public int PageSize { get; set; }
}