using Analite.Application.Dtos.Create;
using Analite.Application.Dtos.Get;

namespace Analite.Application.Interfaces;

public interface ICustomerService
{
	Task<CustomerGetDto> RegisterCustomerAsync(CustomerCreateDto customerCreateDto);
	Task<string> LoginCustomerAsync(string email, string password);

	Task<string> RefreshTokenAsync(string token);

	Task<CustomerGetDto> GetById(Guid customerId);


	Task UpdateCustomerAsync(Guid customerId, CustomerCreateDto customerCreateDto);

	Task<bool> IsApprovedAsync(Guid customerId);
	Task<bool> IsActiveAsync(Guid customerId);

	Task ResetPublicKey(Guid customerId);
}