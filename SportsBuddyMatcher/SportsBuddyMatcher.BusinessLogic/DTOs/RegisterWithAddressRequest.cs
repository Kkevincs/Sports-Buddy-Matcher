using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public class RegisterWithAddressRequest
{
    [Required, MaxLength(255)]
    public required string Name { get; set; }
    [Required, EmailAddress, MaxLength(255)]
    public required string Email { get; set; }
    [Required, MaxLength(500)]
    public required string Password { get; set; }
    [Range(0, int.MaxValue)]
    public decimal? PreferredRangeKm { get; set; }
    public required LocationRequest Address { get; set; }
}
