using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.Repository.Interfaces;

public interface IPlayerSkillRepository : IRepository<PlayerSkill>
{
    Task<List<PlayerSkill>> GetForUserAsync(int userId);
    Task<PlayerSkill?> GetForUserByIdAsync(int userId, int id);
    Task<List<PlayerSkill>> GetForSportAsync(int skillId);
}
