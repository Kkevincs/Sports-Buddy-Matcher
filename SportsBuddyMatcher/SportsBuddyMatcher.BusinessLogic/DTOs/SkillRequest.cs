using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public class SkillRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; set; }
}
