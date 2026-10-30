using Microsoft.AspNetCore.Mvc;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.BusinessLogic.DTOs;

namespace SportsBuddyMatcher.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _service;

    public SkillsController(ISkillService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<SkillDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SkillDto>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null)
            return NotFound();

        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<SkillDto>> Create(SkillRequest request)
    {
        var entity = await _service.AddAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<List<SkillDto>>> CreateBulk(List<SkillRequest> requests)
    {
        var skills = await _service.AddRangeAsync(requests);
        return CreatedAtAction(nameof(GetAll), skills);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SkillRequest request)
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
