namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public int Location { get; set; }
    public decimal? PreferredRangeKm { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public List<AvailabilityDto> Availabilities { get; set; } = new();
}
