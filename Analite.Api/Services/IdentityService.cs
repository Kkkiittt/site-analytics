using System.Security.Claims;

using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Domain.Exceptions;

namespace Analite.Api.Services;

public class IdentityService : IIdentityService
{
	private readonly IHttpContextAccessor _acc;

	public IdentityService(IHttpContextAccessor acc)
	{
		_acc = acc;
	}

	public Guid Id
	{
		get
		{
			return Guid.Parse(_acc.HttpContext?.User.Claims
				.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedException("Id in token"));
		}
	}

	public Roles Role
	{
		get
		{
			string roleStr = _acc.HttpContext?.User.Claims
				.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? throw new UnauthorizedException("Role in token");
			return Enum.Parse<Roles>(roleStr);
		}
	}

	public bool IsAdmin
	{
		get
		{
			return Role == Roles.Admin || Role == Roles.SuperAdmin;
		}
	}

	public bool IsApproved
	{
		get
		{
			string approvedStr = _acc.HttpContext?.User.Claims
				.FirstOrDefault(c => c.Type == "Approved")?.Value ?? throw new UnauthorizedException("Approvedness in token");
			return bool.Parse(approvedStr);
		}
	}

	public string SecurityStamp
	{
		get
		{
			return _acc.HttpContext?.User.Claims
				.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value ?? throw new UnauthorizedException("SecurityStamp in token");
		}
	}
}