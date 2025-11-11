using Analite.Application.Dtos.Create;

namespace Analite.Application.Interfaces;

public interface IEventService
{
	Task CollectAsync(EventCreateDto dto);
}