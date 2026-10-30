using AutoMapper;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.BusinessLogic.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public EventService(IEventRepository repository, IMapper mapper, ICurrentUser currentUser)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<List<EventDto>> GetAllAsync()
    {
        return _mapper.Map<List<EventDto>>(await _repository.GetWithParticipantsAsync());
    }

    public async Task<EventDto?> GetByIdAsync(int id)
    {
        return _mapper.Map<EventDto?>(await _repository.GetWithParticipantsAsync(id));
    }

    public async Task<EventDto> AddAsync(EventRequest request)
    {
        var entity = _mapper.Map<Event>(request);
        entity.CreatedById = _currentUser.Id;
        await _repository.AddAsync(entity);
        return _mapper.Map<EventDto>(entity);
    }

    public async Task UpdateAsync(int id, EventRequest request)
    {
        var entity = await OwnedEventAsync(id);
        _mapper.Map(request, entity);
        await _repository.UpdateWithCapacityAsync(entity);
    }
    public async Task DeleteAsync(int id)
    {
        await OwnedEventAsync(id);
        await _repository.DeleteAsync(id);
    }

    public async Task SetPhotoAsync(int id, string url)
    {
        var item = await OwnedEventAsync(id);
        item.PictureUrl = url;
        await _repository.UpdateAsync(item);
    }

    private async Task<Event> OwnedEventAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Event not found.");
        if (item.CreatedById != _currentUser.Id) throw new UnauthorizedAccessException();
        return item;
    }

    public async Task<List<EventDto>> GetJoinedAsync()
    {
        return _mapper.Map<List<EventDto>>(await _repository.GetJoinedAsync(_currentUser.Id));
    }
}
