using Microsoft.EntityFrameworkCore;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.Repository.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly SportsBuddyMatcherDbContext _db;
    public UserRepository(SportsBuddyMatcherDbContext db) : base(db) { _db = db; }

    public Task<User?> GetWithAvailabilityAsync(int id) =>
        _db.Users.Include(u => u.Availabilities).FirstOrDefaultAsync(u => u.Id == id);

    public Task<List<User>> GetAllWithAvailabilityAsync() =>
        _db.Users.Include(u => u.Availabilities).ToListAsync();

    public Task<PublicProfileData?> GetProfileAsync(int id) => _db.Users.AsNoTracking().Where(u => u.Id == id)
        .Select(u => new PublicProfileData(u.Id, u.Name, u.Bio, u.ProfilePictureUrl,
            u.LocationNavigation.City, u.LocationNavigation.Country,
            u.PlayerSkills.Select(p => new PublicSportData(p.SkillId, p.Skill.Name, p.SkillLevel)).ToList()))
        .FirstOrDefaultAsync();
}
