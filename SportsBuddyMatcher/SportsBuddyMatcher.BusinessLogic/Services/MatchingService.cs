using System.Globalization;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.BusinessLogic.Services;

public class MatchingService : IMatchingService
{
    private readonly IPlayerSkillRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly IEventRepository _events;
    private readonly TimeProvider _clock;

    public MatchingService(IPlayerSkillRepository repository, ICurrentUser currentUser, IEventRepository events, TimeProvider clock)
    {
        _repository = repository;
        _currentUser = currentUser;
        _events = events;
        _clock = clock;
    }

    public async Task<List<MatchDto>> GetMatchesAsync(int skillId)
    {
        return ScoreUsers(await _repository.GetForSportAsync(skillId));
    }

    private List<MatchDto> ScoreUsers(List<PlayerSkill> players)
    {
        var me = players.FirstOrDefault(p => p.UserId == _currentUser.Id);
        if (me is null || me.User.PreferredRangeKm is null || !TryCoordinates(me.User.LocationNavigation, out var myLat, out var myLon))
            return new List<MatchDto>();

        var myTimes = MergeTimes(me.User.Availabilities);
        var matches = new List<MatchDto>();
        foreach (var other in players.Where(p => p.UserId != me.UserId))
        {
            if (other.User.PreferredRangeKm is null || !TryCoordinates(other.User.LocationNavigation, out var lat, out var lon))
                continue;

            var range = (double)Math.Min(me.User.PreferredRangeKm.Value, other.User.PreferredRangeKm.Value);
            var distance = DistanceKm(myLat, myLon, lat, lon);
            if (distance > range) continue;

            var theirTimes = MergeTimes(other.User.Availabilities);
            var shared = SharedMinutes(myTimes, theirTimes);
            if (shared <= 0) continue;

            var skill = 100 - 25 * Math.Abs(me.SkillLevel - other.SkillLevel);
            var location = range == 0 ? 100 : 100 * (1 - distance / range);
            var union = myTimes.Sum(t => t.End - t.Start) + theirTimes.Sum(t => t.End - t.Start) - shared;
            var availability = 100 * shared / union;
            var score = 0.5 * skill + 0.3 * location + 0.2 * availability;
            matches.Add(new MatchDto(other.UserId, other.User.Name, score, Math.Round(distance, 1)));
        }

        return matches.OrderByDescending(m => m.Score).ThenBy(m => m.UserId)
            .Select(m => m with { Score = Math.Round(m.Score, 2) }).ToList();
    }

    public async Task<List<FeedMatchDto>> GetFeedAsync(int skillId)
    {
        var players = await _repository.GetForSportAsync(skillId);
        var byUser = players.ToDictionary(p => p.UserId);
        var feed = ScoreUsers(players).Select(m => new FeedMatchDto("user", m.UserId, m.Name, m.Score,
            Bio: byUser[m.UserId].User.Bio, PictureUrl: byUser[m.UserId].User.ProfilePictureUrl,
            City: byUser[m.UserId].User.LocationNavigation.City, DistanceKm: m.DistanceKm, SkillLevel: byUser[m.UserId].SkillLevel)).ToList();
        var me = players.FirstOrDefault(p => p.UserId == _currentUser.Id);
        if (me is null || me.User.PreferredRangeKm is null || !TryCoordinates(me.User.LocationNavigation, out var myLat, out var myLon))
            return feed;

        var times = MergeTimes(me.User.Availabilities);
        var now = TimeZoneInfo.ConvertTime(_clock.GetUtcNow(), TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest")).DateTime;
        var events = await _events.GetUpcomingForSportAsync(skillId, now);
        foreach (var item in events)
        {
            if (item.UserEvents.Any(r => r.UserId == _currentUser.Id)) continue;
            if (item.SkillLevel is null || item.EndDate is null || item.EndDate <= item.Date
                || !TryCoordinates(item.LocationNavigation, out var lat, out var lon)) continue;

            var range = (double)me.User.PreferredRangeKm.Value;
            var distance = DistanceKm(myLat, myLon, lat, lon);
            if (distance > range) continue;

            var availability = EventAvailability(times, item.Date, item.EndDate.Value);
            if (availability <= 0) continue;

            var skill = 100 - 25 * Math.Abs(me.SkillLevel - item.SkillLevel.Value);
            var location = range == 0 ? 100 : 100 * (1 - distance / range);
            var score = Math.Round(0.5 * skill + 0.3 * location + 0.2 * availability, 2);
            var address = item.LocationNavigation;
            feed.Add(new FeedMatchDto("event", item.Id, item.Name, score, item.Date, item.EndDate,
                $"{address.Street}, {address.Postcode}, {address.City}, {address.Country}", item.MaxParticipants, item.UserEvents.Count,
                item.Bio, item.PictureUrl, address.City, Math.Round(distance, 1), item.SkillLevel));
        }
        return feed.OrderByDescending(m => m.Score).ThenBy(m => m.Type).ThenBy(m => m.Id).ToList();
    }

    private static double EventAvailability(List<(double Start, double End)> times, DateTime start, DateTime end)
    {
        const double week = 7 * 1440;
        var duration = (end - start).TotalMinutes;
        var shared = Math.Floor(duration / week) * times.Sum(t => t.End - t.Start);
        var from = (int)start.DayOfWeek * 1440 + start.TimeOfDay.TotalMinutes;
        var until = from + duration % week;
        shared += SharedMinutes(times, new() { (from, Math.Min(until, week)) });
        if (until > week) shared += SharedMinutes(times, new() { (0, until - week) });
        return 100 * shared / duration;
    }

    private static bool TryCoordinates(Location location, out double latitude, out double longitude)
    {
        latitude = longitude = 0;
        return double.TryParse(location.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out latitude)
            && double.TryParse(location.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out longitude)
            && latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
    }

    private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double radians = Math.PI / 180;
        var a = Math.Pow(Math.Sin((lat2 - lat1) * radians / 2), 2)
            + Math.Cos(lat1 * radians) * Math.Cos(lat2 * radians) * Math.Pow(Math.Sin((lon2 - lon1) * radians / 2), 2);
        return 6371 * 2 * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));
    }

    private static List<(double Start, double End)> MergeTimes(IEnumerable<Availability> times)
    {
        var sorted = times.Where(t => t.EndTime > t.StartTime)
            .Select(t => (Start: (int)t.DayOfWeek * 1440 + t.StartTime.ToTimeSpan().TotalMinutes,
                          End: (int)t.DayOfWeek * 1440 + t.EndTime.ToTimeSpan().TotalMinutes))
            .OrderBy(t => t.Start).ToList();
        var merged = new List<(double Start, double End)>();
        foreach (var time in sorted)
        {
            if (merged.Count == 0 || time.Start > merged[^1].End)
                merged.Add(time);
            else
                merged[^1] = (merged[^1].Start, Math.Max(merged[^1].End, time.End));
        }
        return merged;
    }

    private static double SharedMinutes(List<(double Start, double End)> first, List<(double Start, double End)> second)
    {
        double shared = 0;
        int i = 0, j = 0;
        while (i < first.Count && j < second.Count)
        {
            shared += Math.Max(0, Math.Min(first[i].End, second[j].End) - Math.Max(first[i].Start, second[j].Start));
            if (first[i].End < second[j].End) i++;
            else j++;
        }
        return shared;
    }
}
