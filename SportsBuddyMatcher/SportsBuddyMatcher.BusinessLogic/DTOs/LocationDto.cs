namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public class LocationDto
{
    public int Id { get; set; }
    public required string Country { get; set; }
    public required string City { get; set; }
    public required string Street { get; set; }
    public required string Postcode { get; set; }
    public string? Longitude { get; set; }
    public string? Latitude { get; set; }
}
