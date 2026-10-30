using AutoMapper;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.BusinessLogic.Services;

public class UserEventService : IUserEventService
{
    private readonly IUserEventRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public UserEventService(IUserEventRepository repository, IMapper mapper, ICurrentUser currentUser)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<List<UserEventDto>> GetAllAsync()
    {
        return _mapper.Map<List<UserEventDto>>(await _repository.GetAllAsync(e => e.UserId == _currentUser.Id));
    }

    public async Task<UserEventDto?> GetByIdAsync(int id)
    {
        return _mapper.Map<UserEventDto?>(await _repository.GetFirstAsync(e => e.Id == id && e.UserId == _currentUser.Id));
    }

    public async Task<UserEventDto> AddAsync(UserEventRequest request)
    {
        var entity = await _repository.JoinAsync(_currentUser.Id, request.EventId);
        return _mapper.Map<UserEventDto>(entity);
    }

    public async Task<bool> UpdateAsync(int id, UserEventRequest request)
    {
        return await _repository.MoveAsync(id, _currentUser.Id, request.EventId);
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetFirstAsync(e => e.Id == id && e.UserId == _currentUser.Id);
        if (entity is null) return false;
        await _repository.DeleteAsync(id);
        return true;
    }
}
