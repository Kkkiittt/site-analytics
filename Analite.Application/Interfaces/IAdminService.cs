using Analite.Application.Dtos.Create;
using Analite.Application.Dtos.Get;
using Analite.Domain.Entities;

namespace Analite.Application.Interfaces;

public interface IAdminService
{
    Task ApproveCustomerAsync(Guid customerId);
    Task BlockCustomerAsync(Guid customerId);
    Task UnblockCustomerAsync(Guid customerId);
    
    Task<IEnumerable<CustomerGetDto>> GetAllCustomersAsync();
    Task<CustomerGetDto?> GetByIdAsync(Guid customerId);
}