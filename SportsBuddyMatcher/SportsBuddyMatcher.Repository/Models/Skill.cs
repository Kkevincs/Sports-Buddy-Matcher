using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.Repository.Models;

public class Skill
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public required string Name { get; set; }
    public ICollection<Event> Events { get; set; } = new List<Event>();

    public ICollection<PlayerSkill> PlayerSkills { get; set; } = new List<PlayerSkill>();
}
