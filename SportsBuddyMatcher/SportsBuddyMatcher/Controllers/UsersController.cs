using Microsoft.AspNetCore.Mvc;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.BusinessLogic.DTOs;

namespace SportsBuddyMatcher.Controllers;

[ApiController]
[Route("api/[controller]/me")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    private readonly ICurrentUser _currentUser;

    public UsersController(IUserService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("/api/Users/{id:int}")]
    public async Task<ActionResult<PublicProfileDto>> GetProfile(int id)
    {
        var profile = await _service.GetProfileAsync(id);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPatch("profile")]
    public async Task<IActionResult> UpdateProfile(ProfileRequest request)
    {
        return await _service.UpdateProfileAsync(_currentUser.Id, request) ? NoContent() : NotFound();
    }

    [HttpPost("photo")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadPhoto(IFormFile file, [FromServices] IPhotoStorage photos)
    {
        using var content = new MemoryStream();
        await file.CopyToAsync(content);
        try
        {
            var url = await photos.SaveAsync(content.ToArray());
            return await _service.SetPhotoAsync(_currentUser.Id, url) ? Ok(new { profilePictureUrl = url }) : NotFound();
        }
        catch (ArgumentException exception) { return BadRequest(exception.Message); }
    }

    [HttpGet]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var user = await _service.GetByIdAsync(_currentUser.Id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UserRequest request)
    {
        await _service.UpdateAsync(_currentUser.Id, request);
        return NoContent();
    }

    [HttpPut("availability")]
    public async Task<IActionResult> UpdateAvailability(List<AvailabilityRequest> request)
    {
        return await _service.UpdateAvailabilityAsync(_currentUser.Id, request) ? NoContent() : NotFound();
    }

    [HttpPatch("preferences")]
    public async Task<IActionResult> UpdatePreferences(PreferencesRequest request)
    {
        return await _service.UpdatePreferencesAsync(_currentUser.Id, request) ? NoContent() : NotFound();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        await _service.DeleteAsync(_currentUser.Id);
        return NoContent();
    }
}
