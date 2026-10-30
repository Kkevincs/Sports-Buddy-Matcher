namespace SportsBuddyMatcher.BusinessLogic.Interfaces;

public interface IGeocodingService
{
    Task<(string? Latitude, string? Longitude)> GetCoordinatesAsync(string country, string city, string street, string postcode);
}
