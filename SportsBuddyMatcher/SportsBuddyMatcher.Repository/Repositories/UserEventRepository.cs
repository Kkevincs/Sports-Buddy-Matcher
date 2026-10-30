using Microsoft.EntityFrameworkCore;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.Repository.Repositories;

public class UserEventRepository : Repository<UserEvent>, IUserEventRepository
{
    private readonly SportsBuddyMatcherDbContext _db;
    public UserEventRepository(SportsBuddyMatcherDbContext db) : base(db) { _db = db; }

    public async Task<UserEvent> JoinAsync(int userId, int eventId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var item = await LockEventAsync(eventId);
        var existing = await _db.UserEvents.FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == eventId);
        if (existing is not null) return existing;
        await CheckCapacityAsync(item);
        var registration = new UserEvent { UserId = userId, EventId = eventId };
        _db.UserEvents.Add(registration);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return registration;
    }

    public async Task<bool> MoveAsync(int id, int userId, int eventId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var registration = await _db.UserEvents.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        if (registration is null) return false;
        if (registration.EventId == eventId) return true;
        var item = await LockEventAsync(eventId);
        if (await _db.UserEvents.AnyAsync(r => r.UserId == userId && r.EventId == eventId))
            throw new InvalidOperationException("You have already joined that event.");
        await CheckCapacityAsync(item);
        registration.EventId = eventId;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    private async Task<Event> LockEventAsync(int eventId)
    {
        // Serialize registrations for this event so simultaneous joins cannot overbook it.
        return await _db.Events.FromSqlInterpolated($"SELECT * FROM [Event] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {eventId}")
            .SingleOrDefaultAsync() ?? throw new KeyNotFoundException("Event not found.");
    }

    private async Task CheckCapacityAsync(Event item)
    {
        if (item.MaxParticipants.HasValue && await _db.UserEvents.CountAsync(r => r.EventId == item.Id) >= item.MaxParticipants.Value)
            throw new InvalidOperationException("This event is full.");
    }
}
