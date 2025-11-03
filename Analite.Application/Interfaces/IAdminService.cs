using Analite.Domain.Entities;

namespace Analite.Application.Interfaces;

public interface IAdminService
{
    Task ApproveCustomerAsync(Guid customerId);
    Task BlockCustomerAsync(Guid customerId);
    Task UnblockCustomerAsync(Guid customerId);
    
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<Customer?> GetByIdAsync(Guid customerId);
}