using Microsoft.EntityFrameworkCore;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.Repository.Repositories;

public class PlayerSkillRepository : Repository<PlayerSkill>, IPlayerSkillRepository
{
    private readonly SportsBuddyMatcherDbContext _db;

    public PlayerSkillRepository(SportsBuddyMatcherDbContext db) : base(db) { _db = db; }

    public Task<List<PlayerSkill>> GetForUserAsync(int userId) =>
        _db.PlayerSkills.Where(p => p.UserId == userId).ToListAsync();

    public Task<PlayerSkill?> GetForUserByIdAsync(int userId, int id) =>
        _db.PlayerSkills.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == id);

    public Task<List<PlayerSkill>> GetForSportAsync(int skillId) =>
        _db.PlayerSkills.AsNoTracking().AsSplitQuery()
            .Include(p => p.User).ThenInclude(u => u.Availabilities)
            .Include(p => p.User).ThenInclude(u => u.LocationNavigation)
            .Where(p => p.SkillId == skillId
                && p.User.LocationNavigation.Latitude != null
                && p.User.LocationNavigation.Longitude != null).ToListAsync();
}
