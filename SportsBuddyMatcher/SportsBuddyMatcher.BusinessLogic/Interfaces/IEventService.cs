using SportsBuddyMatcher.BusinessLogic.DTOs;

namespace SportsBuddyMatcher.BusinessLogic.Interfaces;

public interface IEventService
{
    Task<List<EventDto>> GetAllAsync();
    Task<List<EventDto>> GetJoinedAsync();
    Task SetPhotoAsync(int id, string url);
    Task<EventDto?> GetByIdAsync(int id);
    Task<EventDto> AddAsync(EventRequest request);
    Task UpdateAsync(int id, EventRequest request);
    Task DeleteAsync(int id);
}
