using Analite.Application.Dtos;
using Analite.Application.Dtos.Get;

namespace Analite.Application.Interfaces;

public interface IAdminService
{
	Task ApproveCustomerAsync(Guid customerId);
	Task BlockCustomerAsync(Guid customerId);
	Task UnblockCustomerAsync(Guid customerId);

	Task<ManyDto<CustomerGetFullDto>> GetAllCustomersAsync(PaginationData pagination);
	Task<CustomerGetFullDto?> GetByIdAsync(Guid customerId);

	Task PromoteCustomerAsync(Guid customerId);
}