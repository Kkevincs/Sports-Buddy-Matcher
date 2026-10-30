using SportsBuddyMatcher.BusinessLogic.DTOs;
namespace SportsBuddyMatcher.BusinessLogic.Interfaces;
public interface IConnectionService
{
    Task<List<ConnectionDto>> GetAllAsync();
    Task<ConnectionDto> RequestAsync(int targetUserId);
    Task<bool> AcceptAsync(int id);
    Task<bool> RemoveAsync(int id);
}
