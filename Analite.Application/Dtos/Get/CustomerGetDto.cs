using Analite.Domain.Entities;

namespace Analite.Application.Dtos.Get;

public class CustomerGetDto
{
	public Guid Id { get; set; }
	public string PublicKey { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Surname { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public Roles Role { get; set; }
	public DateTime CreatedAt { get; set; }
}