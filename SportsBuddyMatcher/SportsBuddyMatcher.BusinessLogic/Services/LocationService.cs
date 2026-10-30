using AutoMapper;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.BusinessLogic.Services;

public class LocationService : ILocationService
{
    private readonly IRepository<Location> _repository;
    private readonly IMapper _mapper;
    private readonly IGeocodingService _geocodingService;

    public LocationService(IRepository<Location> repository, IMapper mapper, IGeocodingService geocodingService)
    {
        _repository = repository;
        _mapper = mapper;
        _geocodingService = geocodingService;
    }

    public async Task<List<LocationDto>> GetAllAsync()
    {
        return _mapper.Map<List<LocationDto>>(await _repository.GetAllAsync());
    }

    public async Task<LocationDto?> GetByIdAsync(int id)
    {
        return _mapper.Map<LocationDto?>(await _repository.GetByIdAsync(id));
    }

    public async Task<LocationDto> AddAsync(LocationRequest request)
    {
        var entity = _mapper.Map<Location>(request);
        (entity.Latitude, entity.Longitude) = await _geocodingService.GetCoordinatesAsync(entity.Country, entity.City, entity.Street, entity.Postcode);
        await _repository.AddAsync(entity);
        return _mapper.Map<LocationDto>(entity);
    }

    public async Task UpdateAsync(int id, LocationRequest request)
    {
        var entity = _mapper.Map<Location>(request);
        entity.Id = id;
        (entity.Latitude, entity.Longitude) = await _geocodingService.GetCoordinatesAsync(entity.Country, entity.City, entity.Street, entity.Postcode);
        await _repository.UpdateAsync(entity);
    }
    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
}
