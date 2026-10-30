using SportsBuddyMatcher.BusinessLogic.DTOs;

namespace SportsBuddyMatcher.BusinessLogic.Interfaces;

public interface IMatchingService
{
    Task<List<MatchDto>> GetMatchesAsync(int skillId);
    Task<List<FeedMatchDto>> GetFeedAsync(int skillId);
}
