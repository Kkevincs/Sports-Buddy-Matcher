using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.Repository.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public required string Name { get; set; }

    [Required, MaxLength(255)]
    public required string Email { get; set; }

    [Required, MaxLength(500)]
    public required string PasswordHash { get; set; }

    public int Location { get; set; }
    public decimal? PreferredRangeKm { get; set; }
    [MaxLength(2000)]
    public string? Bio { get; set; }
    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }
    public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

    public Location LocationNavigation { get; set; }
    public ICollection<PlayerSkill> PlayerSkills { get; set; } = new List<PlayerSkill>();
    public ICollection<UserEvent> UserEvents { get; set; } = new List<UserEvent>();
}
