using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.BusinessLogic.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<User> _hasher;

    public UserService(IUserRepository repository, IMapper mapper, IPasswordHasher<User> hasher)
    {
        _repository = repository;
        _mapper = mapper;
        _hasher = hasher;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return _mapper.Map<List<UserDto>>(await _repository.GetAllWithAvailabilityAsync());
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        return _mapper.Map<UserDto?>(await _repository.GetWithAvailabilityAsync(id));
    }

    public async Task<UserDto> AddAsync(UserRequest request)
    {
        var entity = _mapper.Map<User>(request);
        entity.PasswordHash = _hasher.HashPassword(entity, request.Password);
        await _repository.AddAsync(entity);
        return _mapper.Map<UserDto>(entity);
    }

    public async Task UpdateAsync(int id, UserRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return;
        _mapper.Map(request, entity);
        entity.PasswordHash = _hasher.HashPassword(entity, request.Password);
        await _repository.UpdateAsync(entity);
    }
    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);

    public async Task<PublicProfileDto?> GetProfileAsync(int id) =>
        _mapper.Map<PublicProfileDto?>(await _repository.GetProfileAsync(id));

    public async Task<bool> UpdateProfileAsync(int id, ProfileRequest request)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null) return false;
        user.Name = request.Name;
        user.Bio = request.Bio;
        await _repository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> SetPhotoAsync(int id, string url)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null) return false;
        user.ProfilePictureUrl = url;
        await _repository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> UpdateAvailabilityAsync(int id, List<AvailabilityRequest> request)
    {
        var user = await _repository.GetWithAvailabilityAsync(id);
        if (user is null) return false;
        _mapper.Map(request, user.Availabilities);
        await _repository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> UpdatePreferencesAsync(int id, PreferencesRequest request)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null) return false;
        user.PreferredRangeKm = request.PreferredRangeKm;
        await _repository.UpdateAsync(user);
        return true;
    }
}
