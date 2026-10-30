using SportsBuddyMatcher.BusinessLogic.DTOs;

namespace SportsBuddyMatcher.BusinessLogic.Interfaces;

public interface IUserEventService
{
    Task<List<UserEventDto>> GetAllAsync();
    Task<UserEventDto?> GetByIdAsync(int id);
    Task<UserEventDto> AddAsync(UserEventRequest request);
    Task<bool> UpdateAsync(int id, UserEventRequest request);
    Task<bool> DeleteAsync(int id);
}
