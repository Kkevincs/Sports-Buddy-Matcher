using SportsBuddyMatcher.BusinessLogic.DTOs;

namespace SportsBuddyMatcher.BusinessLogic.Interfaces;

public interface ISkillService
{
    Task<List<SkillDto>> GetAllAsync();
    Task<SkillDto?> GetByIdAsync(int id);
    Task<SkillDto> AddAsync(SkillRequest request);
    Task<List<SkillDto>> AddRangeAsync(List<SkillRequest> requests);
    Task UpdateAsync(int id, SkillRequest request);
    Task DeleteAsync(int id);
}
