using SportsBuddyMatcher.BusinessLogic.DTOs;

namespace SportsBuddyMatcher.BusinessLogic.Interfaces;

public interface ILocationService
{
    Task<List<LocationDto>> GetAllAsync();
    Task<LocationDto?> GetByIdAsync(int id);
    Task<LocationDto> AddAsync(LocationRequest request);
    Task UpdateAsync(int id, LocationRequest request);
    Task DeleteAsync(int id);
}
