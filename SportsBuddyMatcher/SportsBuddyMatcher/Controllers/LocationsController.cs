using Microsoft.AspNetCore.Mvc;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.BusinessLogic.DTOs;

namespace SportsBuddyMatcher.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _service;

    public LocationsController(ILocationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocationDto>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null)
            return NotFound();

        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<LocationDto>> Create(LocationRequest request)
    {
        var entity = await _service.AddAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, LocationRequest request)
    {
        await _service.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
