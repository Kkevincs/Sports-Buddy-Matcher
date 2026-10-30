using Microsoft.AspNetCore.Mvc;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
namespace SportsBuddyMatcher.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ConnectionsController : ControllerBase
{
    private readonly IConnectionService _service;
    public ConnectionsController(IConnectionService service) { _service = service; }
    [HttpGet]
    public async Task<ActionResult<List<ConnectionDto>>> Get() => Ok(await _service.GetAllAsync());
    [HttpPost]
    public async Task<ActionResult<ConnectionDto>> CreateRequest(ConnectionRequest request)
    {
        try { return Ok(await _service.RequestAsync(request.TargetUserId)); }
        catch (ArgumentException exception) { return BadRequest(exception.Message); }
        catch (KeyNotFoundException exception) { return NotFound(exception.Message); }
    }
    [HttpPost("{id:int}/accept")]
    public async Task<IActionResult> Accept(int id)
    {
        try { return await _service.AcceptAsync(id) ? NoContent() : NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id) => await _service.RemoveAsync(id) ? NoContent() : NotFound();
}
