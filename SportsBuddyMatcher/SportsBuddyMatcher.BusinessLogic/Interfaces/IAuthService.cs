using SportsBuddyMatcher.BusinessLogic.DTOs;
namespace SportsBuddyMatcher.BusinessLogic.Interfaces;
public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(UserRequest request);
    Task<AuthResponse?> RegisterWithAddressAsync(RegisterWithAddressRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}
