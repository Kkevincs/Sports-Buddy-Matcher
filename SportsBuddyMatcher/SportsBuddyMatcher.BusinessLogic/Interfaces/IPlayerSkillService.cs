using SportsBuddyMatcher.BusinessLogic.DTOs;

namespace SportsBuddyMatcher.BusinessLogic.Interfaces;

public interface IPlayerSkillService
{
    Task<List<PlayerSkillDto>> GetAllAsync();
    Task<PlayerSkillDto?> GetByIdAsync(int id);
    Task<PlayerSkillDto> AddAsync(PlayerSkillRequest request);
    Task<bool> UpdateAsync(int id, PlayerSkillRequest request);
    Task<bool> DeleteAsync(int id);
}
