using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.Repository.Interfaces;

public interface IEventRepository : IRepository<Event>
{
    Task<List<Event>> GetUpcomingForSportAsync(int skillId, DateTime now);
    Task<List<Event>> GetWithParticipantsAsync();
    Task<Event?> GetWithParticipantsAsync(int id);
    Task<List<Event>> GetJoinedAsync(int userId);
    Task UpdateWithCapacityAsync(Event item);
}
