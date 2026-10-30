using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;

namespace SportsBuddyMatcher.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly IMatchingService _service;
    public MatchesController(IMatchingService service) { _service = service; }

    [HttpGet("feed")]
    public async Task<ActionResult<List<FeedMatchDto>>> GetFeed([FromQuery, Range(1, int.MaxValue)] int skillId)
    {
        return Ok(await _service.GetFeedAsync(skillId));
    }

    [HttpGet]
    public async Task<ActionResult<List<MatchDto>>> Get([FromQuery, Range(1, int.MaxValue)] int skillId)
    {
        return Ok(await _service.GetMatchesAsync(skillId));
    }
}
