using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.Repository.Interfaces;

public interface IUserEventRepository : IRepository<UserEvent>
{
    Task<UserEvent> JoinAsync(int userId, int eventId);
    Task<bool> MoveAsync(int id, int userId, int eventId);
}
