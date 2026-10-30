using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SportsBuddyMatcher.BusinessLogic.Interfaces;

namespace SportsBuddyMatcher.BusinessLogic.Services;

public class GeoapifyGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeoapifyGeocodingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GEOAPIFY_API_KEY"]!;
    }

    public async Task<(string? Latitude, string? Longitude)> GetCoordinatesAsync(string country, string city, string street, string postcode)
    {
        var address = Uri.EscapeDataString($"{street}, {city}, {postcode}, {country}");
        var url = $"search?text={address}&format=json&limit=1&apiKey={Uri.EscapeDataString(_apiKey)}";

        try
        {
            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var results = json.RootElement.GetProperty("results");
            if (results.GetArrayLength() == 0)
                return (null, null);

            var result = results[0];
            return (result.GetProperty("lat").GetRawText(), result.GetProperty("lon").GetRawText());
        }
        catch (TaskCanceledException)
        {
            return (null, null);
        }
    }
}
