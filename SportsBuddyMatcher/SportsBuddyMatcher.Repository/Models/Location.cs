using System.ComponentModel.DataAnnotations;

namespace SportsBuddyMatcher.Repository.Models;

public class Location
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public required string Country { get; set; }

    [Required, MaxLength(100)]
    public required string City { get; set; }

    [Required, MaxLength(255)]
    public required string Street { get; set; }

    [Required]
    public required string Postcode { get; set; }

    public string? Longitude { get; set; }
    public string? Latitude { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
