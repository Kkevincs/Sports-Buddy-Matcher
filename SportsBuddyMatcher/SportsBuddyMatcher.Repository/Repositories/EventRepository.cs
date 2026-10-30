using Microsoft.EntityFrameworkCore;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.Repository.Repositories;

public class EventRepository : Repository<Event>, IEventRepository
{
    private readonly SportsBuddyMatcherDbContext _db;
    public EventRepository(SportsBuddyMatcherDbContext db) : base(db) { _db = db; }

    public Task<List<Event>> GetUpcomingForSportAsync(int skillId, DateTime now) =>
        _db.Events.AsNoTracking().AsSplitQuery().Include(e => e.LocationNavigation).Include(e => e.UserEvents)
            .Where(e => e.SkillId == skillId && e.Date > now).ToListAsync();

    public Task<List<Event>> GetWithParticipantsAsync() =>
        _db.Events.AsNoTracking().AsSplitQuery().Include(e => e.UserEvents).ToListAsync();

    public Task<Event?> GetWithParticipantsAsync(int id) =>
        _db.Events.AsNoTracking().Include(e => e.UserEvents).FirstOrDefaultAsync(e => e.Id == id);

    public Task<List<Event>> GetJoinedAsync(int userId) =>
        _db.Events.AsNoTracking().AsSplitQuery().Include(e => e.UserEvents)
            .Where(e => e.UserEvents.Any(r => r.UserId == userId)).ToListAsync();

    public async Task UpdateWithCapacityAsync(Event item)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var existing = await _db.Events.FromSqlInterpolated($"SELECT * FROM [Event] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {item.Id}")
            .SingleOrDefaultAsync() ?? throw new KeyNotFoundException("Event not found.");
        if (item.MaxParticipants.HasValue && await _db.UserEvents.CountAsync(r => r.EventId == item.Id) > item.MaxParticipants.Value)
            throw new InvalidOperationException("Capacity cannot be less than the number of joined participants.");
        _db.Entry(existing).CurrentValues.SetValues(item);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
