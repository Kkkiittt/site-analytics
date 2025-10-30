namespace Analite.Domain.Entities;

public class Customer
{
	public Guid Id { get; set; }
	public string PublicKey { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Surname { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string PasswordHash { get; set; } = string.Empty;
	public string SecurityStamp { get; set; } = string.Empty;
	public Roles Role { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public bool IsApproved { get; set; }
	public bool IsActive { get; set; }
}

public enum Roles
{
	User,
	Admin,
	SuperAdmin
}