using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public class LocationRequest
{
    [Required, MaxLength(100)]
    public required string Country { get; set; }

    [Required, MaxLength(100)]
    public required string City { get; set; }

    [Required, MaxLength(255)]
    public required string Street { get; set; }
    [Required]
    public required string Postcode { get; set; }
}
