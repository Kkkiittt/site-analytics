using Analite.Domain.Entities;

namespace Analite.Application.Interfaces;

public interface IIdentityService
{
	public Guid Id{ get; }
	public Roles Role{ get; }
	public bool IsAdmin{ get; }
	public bool IsApproved{ get; }
	public string SecurityStamp{ get; }
}