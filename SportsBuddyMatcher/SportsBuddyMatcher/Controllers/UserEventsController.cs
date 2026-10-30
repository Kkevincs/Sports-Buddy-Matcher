using Microsoft.AspNetCore.Mvc;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.BusinessLogic.DTOs;

namespace SportsBuddyMatcher.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserEventsController : ControllerBase
{
    private readonly IUserEventService _service;

    public UserEventsController(IUserEventService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserEventDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserEventDto>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null)
            return NotFound();

        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<UserEventDto>> Create(UserEventRequest request)
    {
        try
        {
            var entity = await _service.AddAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }
        catch (KeyNotFoundException exception) { return NotFound(exception.Message); }
        catch (InvalidOperationException exception) { return Conflict(exception.Message); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UserEventRequest request)
    {
        try { return await _service.UpdateAsync(id, request) ? NoContent() : NotFound(); }
        catch (KeyNotFoundException exception) { return NotFound(exception.Message); }
        catch (InvalidOperationException exception) { return Conflict(exception.Message); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
