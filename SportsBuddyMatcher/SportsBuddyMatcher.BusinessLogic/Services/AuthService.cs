using Microsoft.AspNetCore.Identity;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;
namespace SportsBuddyMatcher.BusinessLogic.Services;
public class AuthService : IAuthService
{
    private readonly IRepository<User> _repository;
    private readonly IPasswordHasher<User> _hasher;
    private readonly ITokenService _tokens;
    private readonly IGeocodingService _geocoding;
    public AuthService(IRepository<User> repository, IPasswordHasher<User> hasher, ITokenService tokens, IGeocodingService geocoding)
    {
        _repository = repository;
        _hasher = hasher;
        _tokens = tokens;
        _geocoding = geocoding;
    }
    public async Task<AuthResponse?> RegisterAsync(UserRequest request)
    {
        if (await _repository.GetFirstAsync(u => u.Email == request.Email) is not null)
            return null;
        var user = new User { Name = request.Name, Email = request.Email, Location = request.Location, PreferredRangeKm = request.PreferredRangeKm, PasswordHash = "" };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        await _repository.AddAsync(user);
        return _tokens.CreateToken(user);
    }
    public async Task<AuthResponse?> RegisterWithAddressAsync(RegisterWithAddressRequest request)
    {
        if (await _repository.GetFirstAsync(u => u.Email == request.Email) is not null)
            return null;
        var address = request.Address;
        var location = new Location { Country = address.Country, City = address.City, Street = address.Street, Postcode = address.Postcode };
        (location.Latitude, location.Longitude) = await _geocoding.GetCoordinatesAsync(address.Country, address.City, address.Street, address.Postcode);
        var user = new User
        {
            Name = request.Name, Email = request.Email, PasswordHash = "",
            PreferredRangeKm = request.PreferredRangeKm, LocationNavigation = location
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        await _repository.AddAsync(user);
        return _tokens.CreateToken(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _repository.GetFirstAsync(u => u.Email == request.Email);
        if (user is null)
            return null;

        try
        {
            if (_hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
                return null;
        }
        catch (FormatException)
        {
            return null;
        }

        return _tokens.CreateToken(user);
    }
}
