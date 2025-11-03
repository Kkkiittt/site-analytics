using Analite.Application.Dtos.Create;
using Analite.Domain.Entities;

namespace Analite.Application.Interfaces;

public interface ICustomerService
{
    Task<Customer> RegisterCustomerAsync(CustomerCreateDto customerCreateDto);
    Task<string> LoginCustomerAsync(string email, string password);
    Task<Customer?> GetById(Guid customerId);

    
    Task UpdateCustomerAsync(Guid customerId , CustomerCreateDto customerCreateDto);
    Task DeleteCustomerAsync(Guid customerId);
    
    Task<bool> IsApprovedAsync(Guid customerId);
    Task<bool> IsActiveAsync(Guid customerId);
    
}