using AutoMapper;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.BusinessLogic.Services;

public class PlayerSkillService : IPlayerSkillService
{
    private readonly IPlayerSkillRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public PlayerSkillService(IPlayerSkillRepository repository, IMapper mapper, ICurrentUser currentUser)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<List<PlayerSkillDto>> GetAllAsync()
    {
        return _mapper.Map<List<PlayerSkillDto>>(await _repository.GetForUserAsync(_currentUser.Id));
    }

    public async Task<PlayerSkillDto?> GetByIdAsync(int id)
    {
        return _mapper.Map<PlayerSkillDto?>(await _repository.GetForUserByIdAsync(_currentUser.Id, id));
    }

    public async Task<PlayerSkillDto> AddAsync(PlayerSkillRequest request)
    {
        var entity = _mapper.Map<PlayerSkill>(request);
        entity.UserId = _currentUser.Id;
        await _repository.AddAsync(entity);
        return _mapper.Map<PlayerSkillDto>(entity);
    }

    public async Task<bool> UpdateAsync(int id, PlayerSkillRequest request)
    {
        var entity = await _repository.GetForUserByIdAsync(_currentUser.Id, id);
        if (entity is null) return false;
        _mapper.Map(request, entity);
        await _repository.UpdateAsync(entity);
        return true;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetForUserByIdAsync(_currentUser.Id, id);
        if (entity is null) return false;
        await _repository.DeleteAsync(id);
        return true;
    }
}
