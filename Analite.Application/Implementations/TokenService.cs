using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Analite.Application.Interfaces;
using Analite.Domain.Entities;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Analite.Application.Implementations;

public class TokenService : ITokenService
{
	private readonly IConfiguration _conf;

	public TokenService(IConfiguration jwtConfiguration)
	{
		_conf = jwtConfiguration;
	}

	public string GenerateToken(Customer entity)
	{
		Claim[] claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, entity.Id.ToString()),
			new Claim(ClaimTypes.Role, entity.Role.ToString()),
			new Claim("Approved", entity.IsApproved.ToString()),
			new Claim(ClaimTypes.Sid, entity.SecurityStamp)
		};

		SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_conf["Key"] ?? Guid.NewGuid().ToString()));

		SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		JwtSecurityToken token = new JwtSecurityToken(
			issuer: _conf["Issuer"],
			audience: _conf["Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddDays(double.Parse(_conf["Lifetime"] ?? "1")),
			signingCredentials: creds
		);

		JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

		return handler.WriteToken(token);
	}
}