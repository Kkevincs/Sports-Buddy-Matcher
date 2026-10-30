using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.Repository.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetWithAvailabilityAsync(int id);
    Task<List<User>> GetAllWithAvailabilityAsync();
    Task<PublicProfileData?> GetProfileAsync(int id);
}
