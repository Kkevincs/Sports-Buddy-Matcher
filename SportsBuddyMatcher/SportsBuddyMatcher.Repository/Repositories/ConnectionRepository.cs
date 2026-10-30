using Microsoft.EntityFrameworkCore;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;
namespace SportsBuddyMatcher.Repository.Repositories;
public class ConnectionRepository : Repository<UserConnection>, IConnectionRepository
{
    private readonly SportsBuddyMatcherDbContext _db;
    public ConnectionRepository(SportsBuddyMatcherDbContext db) : base(db) { _db = db; }
    public async Task<UserConnection> CreateOrGetAsync(UserConnection connection)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        await _db.Users.FromSqlInterpolated($"SELECT * FROM [User] WITH (UPDLOCK, HOLDLOCK) WHERE Id = {connection.UserId}").SingleAsync();
        var existing = await _db.Connections.FirstOrDefaultAsync(c => c.UserId == connection.UserId && c.OtherUserId == connection.OtherUserId);
        if (existing is not null) return existing;
        _db.Connections.Add(connection);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return connection;
    }
    public Task<List<ConnectionData>> GetForUserAsync(int userId) =>
        _db.Connections.AsNoTracking().Where(c => c.UserId == userId || c.OtherUserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ConnectionData(c.Id, c.UserId == userId ? c.OtherUserId : c.UserId,
                c.RequestedById, c.IsAccepted,
                c.UserId == userId ? c.OtherUser.Name : c.User.Name,
                c.UserId == userId ? c.OtherUser.Bio : c.User.Bio,
                c.UserId == userId ? c.OtherUser.ProfilePictureUrl : c.User.ProfilePictureUrl,
                c.UserId == userId ? c.OtherUser.LocationNavigation.City : c.User.LocationNavigation.City)).ToListAsync();
}
