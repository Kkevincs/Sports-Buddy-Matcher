using Microsoft.AspNetCore.Mvc;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.BusinessLogic.DTOs;

namespace SportsBuddyMatcher.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerSkillsController : ControllerBase
{
    private readonly IPlayerSkillService _service;

    public PlayerSkillsController(IPlayerSkillService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlayerSkillDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlayerSkillDto>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null)
            return NotFound();

        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<PlayerSkillDto>> Create(PlayerSkillRequest request)
    {
        var entity = await _service.AddAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PlayerSkillRequest request)
    {
        return await _service.UpdateAsync(id, request) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
