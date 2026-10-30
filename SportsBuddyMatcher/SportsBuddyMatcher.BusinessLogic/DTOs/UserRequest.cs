using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public class UserRequest
{
    [Required, MaxLength(255)]
    public required string Name { get; set; }

    [Required, MaxLength(255)]
    public required string Email { get; set; }

    [Required, MaxLength(500)]
    public required string Password { get; set; }

    public required int Location { get; set; }
    public decimal? PreferredRangeKm { get; set; }
}
