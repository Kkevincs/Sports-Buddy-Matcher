using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.Repository.Models;

public class Event
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateTime Date { get; set; }
    public DateTime? EndDate { get; set; }
    public int? SkillId { get; set; }
    public int? SkillLevel { get; set; }
    public Skill? Skill { get; set; }
    public int Location { get; set; }
    public int? MaxParticipants { get; set; }
    [MaxLength(4000)]
    public string? Bio { get; set; }
    [MaxLength(500)]
    public string? PictureUrl { get; set; }
    public int? CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    public Location LocationNavigation { get; set; }
    public ICollection<UserEvent> UserEvents { get; set; } = new List<UserEvent>();
}
