using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
namespace SportsBuddyMatcher.Controllers;
[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;
    public AuthController(IAuthService service) { _service = service; }
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(UserRequest request)
    {
        var result = await _service.RegisterAsync(request);
        return result is null ? Conflict("Email is already registered.") : Ok(result);
    }
    [HttpPost("register-with-address")]
    public async Task<ActionResult<AuthResponse>> RegisterWithAddress(RegisterWithAddressRequest request)
    {
        var result = await _service.RegisterWithAddressAsync(request);
        return result is null ? Conflict("Email is already registered.") : Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var result = await _service.LoginAsync(request);
        return result is null ? Unauthorized() : Ok(result);
    }
}
