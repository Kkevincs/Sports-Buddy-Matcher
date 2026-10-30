using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.Repository.Models;

public class PlayerSkill
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SkillId { get; set; }

    [Range(1, 5)]
    public int SkillLevel { get; set; }

    public User User { get; set; }
    public Skill Skill { get; set; }
}
