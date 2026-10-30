using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;
namespace SportsBuddyMatcher.BusinessLogic.Services;
public class ConnectionService : IConnectionService
{
    private readonly IConnectionRepository _repository;
    private readonly IRepository<User> _users;
    private readonly ICurrentUser _current;
    public ConnectionService(IConnectionRepository repository, IRepository<User> users, ICurrentUser current)
    { _repository = repository; _users = users; _current = current; }
    public async Task<List<ConnectionDto>> GetAllAsync() => (await _repository.GetForUserAsync(_current.Id))
        .Select(c => new ConnectionDto(c.Id, c.OtherUserId, c.Name, c.Bio, c.ProfilePictureUrl, c.City,
            c.IsAccepted ? "connected" : "pending", !c.IsAccepted && c.RequestedById != _current.Id)).ToList();
    public async Task<ConnectionDto> RequestAsync(int targetUserId)
    {
        if (targetUserId == _current.Id) throw new ArgumentException("You cannot connect to yourself.");
        if (await _users.GetByIdAsync(targetUserId) is null) throw new KeyNotFoundException("User not found.");
        var connection = await _repository.CreateOrGetAsync(new UserConnection
        { UserId = Math.Min(_current.Id, targetUserId), OtherUserId = Math.Max(_current.Id, targetUserId), RequestedById = _current.Id });
        return (await GetAllAsync()).Single(c => c.Id == connection.Id);
    }
    public async Task<bool> AcceptAsync(int id)
    {
        var connection = await _repository.GetByIdAsync(id);
        if (connection is null || (connection.UserId != _current.Id && connection.OtherUserId != _current.Id)) return false;
        if (connection.RequestedById == _current.Id) throw new UnauthorizedAccessException();
        connection.IsAccepted = true;
        await _repository.UpdateAsync(connection);
        return true;
    }
    public async Task<bool> RemoveAsync(int id)
    {
        var connection = await _repository.GetByIdAsync(id);
        if (connection is null || (connection.UserId != _current.Id && connection.OtherUserId != _current.Id)) return false;
        await _repository.DeleteAsync(id);
        return true;
    }
}
