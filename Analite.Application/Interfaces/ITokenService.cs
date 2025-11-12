using Analite.Domain.Entities;

namespace Analite.Application.Interfaces;

public interface ITokenService
{
	string GenerateToken(Customer entity);

	string GetStamp(string token);
	Guid GetId(string token);

	void ValidateToken(string token);
}