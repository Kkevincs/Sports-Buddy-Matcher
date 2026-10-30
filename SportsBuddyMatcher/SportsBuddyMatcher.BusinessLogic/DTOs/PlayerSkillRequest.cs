using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public class PlayerSkillRequest
{
    public required int SkillId { get; set; }

    [Range(1, 5)]
    public required int SkillLevel { get; set; }
}
