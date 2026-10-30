using Microsoft.AspNetCore.Mvc;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.BusinessLogic.DTOs;

namespace SportsBuddyMatcher.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _service;

    public EventsController(IEventService service)
    {
        _service = service;
    }

    [HttpGet("joined")]
    public async Task<ActionResult<List<EventDto>>> GetJoined()
    {
        return Ok(await _service.GetJoinedAsync());
    }

    [HttpGet]
    public async Task<ActionResult<List<EventDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null)
            return NotFound();

        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<EventDto>> Create(EventRequest request)
    {
        var entity = await _service.AddAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, EventRequest request)
    {
        try
        {
            await _service.UpdateAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException exception) { return NotFound(exception.Message); }
        catch (InvalidOperationException exception) { return Conflict(exception.Message); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _service.DeleteAsync(id); return NoContent(); }
        catch (KeyNotFoundException exception) { return NotFound(exception.Message); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("{id:int}/photo")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file, [FromServices] IPhotoStorage photos, [FromServices] ICurrentUser currentUser)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null) return NotFound();
        if (item.CreatedById != currentUser.Id) return Forbid();
        using var content = new MemoryStream();
        await file.CopyToAsync(content);
        try
        {
            var url = await photos.SaveAsync(content.ToArray());
            await _service.SetPhotoAsync(id, url);
            return Ok(new { pictureUrl = url });
        }
        catch (ArgumentException exception) { return BadRequest(exception.Message); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
