using AutoMapper;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.BusinessLogic.Services;

public class SkillService : ISkillService
{
    private readonly IRepository<Skill> _repository;
    private readonly IMapper _mapper;

    public SkillService(IRepository<Skill> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<SkillDto>> GetAllAsync()
    {
        return _mapper.Map<List<SkillDto>>(await _repository.GetAllAsync());
    }

    public async Task<SkillDto?> GetByIdAsync(int id)
    {
        return _mapper.Map<SkillDto?>(await _repository.GetByIdAsync(id));
    }

    public async Task<SkillDto> AddAsync(SkillRequest request)
    {
        var entity = _mapper.Map<Skill>(request);
        await _repository.AddAsync(entity);
        return _mapper.Map<SkillDto>(entity);
    }

    public async Task<List<SkillDto>> AddRangeAsync(List<SkillRequest> requests)
    {
        var entities = _mapper.Map<List<Skill>>(requests);
        await _repository.AddRangeAsync(entities);
        return _mapper.Map<List<SkillDto>>(entities);
    }

    public Task UpdateAsync(int id, SkillRequest request)
    {
        var entity = _mapper.Map<Skill>(request);
        entity.Id = id;
        return _repository.UpdateAsync(entity);
    }
    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
}
