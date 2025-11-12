using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Domain.Exceptions;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Analite.Application.Implementations;

public class TokenService : ITokenService
{
	private readonly IConfiguration _conf;

	public TokenService(IConfiguration jwtConfiguration)
	{
		_conf = jwtConfiguration.GetSection("Jwt");
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

		SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_conf["Key"] ?? throw new Exception("No Jwt Key!!!")));

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

	public Guid GetId(string token)
	{
		JwtSecurityToken tk = new JwtSecurityToken(token);
		return Guid.Parse(tk.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedException("Id in token"));
	}

	public string GetStamp(string token)
	{
		JwtSecurityToken tk = new JwtSecurityToken(token);
		return tk.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value ?? throw new UnauthorizedException("Security stamp in token");
	}

	public void ValidateToken(string token)
	{
		JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
		var validation = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer=_conf["Issuer"],
			ValidateAudience = false,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_conf["Key"] ?? throw new Exception("No Jwt key!!!"))),
			ValidateIssuerSigningKey = true,
			ValidateLifetime = false
		};
		try
		{
			handler.ValidateToken(token, validation, out _);
		}
		catch(Exception ex)
		{
			//throw ex;
			throw new UnauthorizedException("Token");
		}
	}
}