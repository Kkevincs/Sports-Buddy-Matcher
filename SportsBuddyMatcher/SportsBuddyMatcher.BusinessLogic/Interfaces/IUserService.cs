using SportsBuddyMatcher.BusinessLogic.DTOs;

namespace SportsBuddyMatcher.BusinessLogic.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> AddAsync(UserRequest request);
    Task UpdateAsync(int id, UserRequest request);
    Task DeleteAsync(int id);
    Task<bool> UpdatePreferencesAsync(int id, PreferencesRequest request);
    Task<PublicProfileDto?> GetProfileAsync(int id);
    Task<bool> UpdateProfileAsync(int id, ProfileRequest request);
    Task<bool> SetPhotoAsync(int id, string url);
    Task<bool> UpdateAvailabilityAsync(int id, List<AvailabilityRequest> request);
}
