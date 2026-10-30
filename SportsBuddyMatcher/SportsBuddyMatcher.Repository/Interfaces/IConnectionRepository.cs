using SportsBuddyMatcher.Repository.Models;
namespace SportsBuddyMatcher.Repository.Interfaces;
public interface IConnectionRepository : IRepository<UserConnection>
{
    Task<UserConnection> CreateOrGetAsync(UserConnection connection);
    Task<List<ConnectionData>> GetForUserAsync(int userId);
}
