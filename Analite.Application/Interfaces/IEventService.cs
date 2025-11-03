using Analite.Application.Dtos.Create;
using Analite.Domain.Entities;

namespace Analite.Application.Interfaces;

public interface IEventService
{
    Task RecordEventAsync(EventCreateDto eventCreateDto);
    Task<int> GetEventsCountAsync(Guid customerId);
    
    Task<IEnumerable<Event>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<Event>> GetByPageIdAsync(long customerId);
    
    
}