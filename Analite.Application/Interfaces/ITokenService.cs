using Analite.Domain.Entities;

namespace Analite.Application.Interfaces;

public interface ITokenService
{
	string GenerateToken(Customer entity);
}