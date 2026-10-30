using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.BusinessLogic.Mappings;
using SportsBuddyMatcher.BusinessLogic.Services;
using SportsBuddyMatcher.Repository;
using SportsBuddyMatcher.Repository.Models;
using SportsBuddyMatcher.Repository.Repositories;

// Run from the repository root. This only creates or updates the named demo fixtures.
var apiRoot = Path.Combine(Directory.GetCurrentDirectory(), "SportsBuddyMatcher", "SportsBuddyMatcher");
var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(apiRoot, "appsettings.json")).AddEnvironmentVariables().Build();
var options = new DbContextOptionsBuilder<SportsBuddyMatcherDbContext>().UseSqlServer(config.GetConnectionString("DefaultConnection")).Options;
await using var db = new SportsBuddyMatcherDbContext(options);
var hasher = new PasswordHasher<User>();
const string demoPassword = "MatchDemo-2026!";
var basketball = await Sport("Matching Demo Basketball");
var tennis = await Sport("Matching Demo Tennis");
var nearbyLatitude = (51.4416 + 2 / 6371.0 * 180 / Math.PI).ToString("R", CultureInfo.InvariantCulture);
var fixtures = new[]
{
    new Fixture("you", "Demo You", 3, 10, "51.4416", 2, 18, 20),
    new Fixture("perfect", "Demo Perfect", 3, 15, "51.4416", 2, 18, 20),
    new Fixture("partial", "Demo Partial Overlap", 3, 15, "51.4416", 2, 19, 21),
    new Fixture("nearby", "Demo Nearby", 4, 15, nearbyLatitude, 2, 18, 20),
    new Fixture("different-level", "Demo Different Level", 1, 15, "51.4416", 2, 18, 20),
    new Fixture("far", "Demo Too Far", 3, 100, "51.9416", 2, 18, 20),
    new Fixture("small-range", "Demo Their Range Too Small", 3, 1, nearbyLatitude, 2, 18, 20),
    new Fixture("no-overlap", "Demo No Overlap", 3, 15, "51.4416", 3, 18, 20),
    new Fixture("wrong-sport", "Demo Tennis Only", 4, 15, "51.4416", 2, 18, 20, true),
    new Fixture("no-coordinates", "Demo Missing Coordinates", 3, 15, null, 2, 18, 20),
    new Fixture("no-range", "Demo Missing Range", 3, null, "51.4416", 2, 18, 20),
    new Fixture("no-availability", "Demo Missing Availability", 3, 15, "51.4416", 2, 18, 20),
    new Fixture("touching", "Demo Touching Times Only", 3, 15, "51.4416", 2, 20, 21)
};
var ids = new Dictionary<string, int>();
await using (var transaction = await db.Database.BeginTransactionAsync())
{
    foreach (var fixture in fixtures)
    {
        var email = $"matching-demo.{fixture.Key}@example.test";
        var user = await db.Users.Include(u => u.LocationNavigation).Include(u => u.Availabilities).SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            user = new User
            {
                Name = fixture.Name, Email = email, PasswordHash = "",
                LocationNavigation = new Location { Country = "Netherlands", City = "Eindhoven", Street = $"Demo Street {Array.IndexOf(fixtures, fixture) + 1}", Postcode = "5611 AA" }
            };
            db.Users.Add(user);
        }
        user.Name = fixture.Name;
        user.PreferredRangeKm = fixture.Range;
        user.PasswordHash = hasher.HashPassword(user, demoPassword);
        user.LocationNavigation.Latitude = fixture.Latitude;
        user.LocationNavigation.Longitude = fixture.Latitude is null ? null : "5.4697";
        await db.SaveChangesAsync();
        ids[fixture.Key] = user.Id;
        user.Availabilities.Clear();
        if (fixture.Key != "no-availability") user.Availabilities.Add(Slot(fixture.Day, fixture.Start, fixture.End));
        await SetPlayerSkill(user.Id, fixture.Tennis ? tennis.Id : basketball.Id, fixture.Level);
        if (fixture.Key == "you")
        {
            // A duplicate overlapping slot must not increase the availability score.
            user.Availabilities.Add(new Availability { DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeOnly(18, 30), EndTime = new TimeOnly(19, 30) });
            await SetPlayerSkill(user.Id, tennis.Id, 1);
        }
    }
    await db.SaveChangesAsync();
    await transaction.CommitAsync();
}
db.ChangeTracker.Clear();

var locations = await db.Users.Where(u => ids.Values.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Location);
var today = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest")).Date;
var daysToTuesday = ((int)DayOfWeek.Tuesday - (int)today.DayOfWeek + 7) % 7;
var eventStart = today.AddDays(daysToTuesday == 0 ? 7 : daysToTuesday).AddHours(18);
var eventFixtures = new[]
{
    new DemoEvent("Perfect", eventStart, eventStart.AddHours(2), basketball.Id, 3, locations[ids["you"]]),
    new DemoEvent("Partial Attendance", eventStart.AddHours(1), eventStart.AddHours(3), basketball.Id, 3, locations[ids["you"]]),
    new DemoEvent("Overnight", eventStart.AddHours(1), eventStart.AddHours(7), basketball.Id, 3, locations[ids["you"]]),
    new DemoEvent("Nearby", eventStart, eventStart.AddHours(2), basketball.Id, 4, locations[ids["nearby"]]),
    new DemoEvent("Too Far", eventStart, eventStart.AddHours(2), basketball.Id, 3, locations[ids["far"]]),
    new DemoEvent("No Overlap", eventStart.AddDays(1), eventStart.AddDays(1).AddHours(2), basketball.Id, 3, locations[ids["you"]]),
    new DemoEvent("Tennis", eventStart, eventStart.AddHours(2), tennis.Id, 4, locations[ids["you"]]),
    new DemoEvent("Past", eventStart.AddDays(-14), eventStart.AddDays(-14).AddHours(2), basketball.Id, 3, locations[ids["you"]]),
    new DemoEvent("Missing End", eventStart, null, basketball.Id, 3, locations[ids["you"]]),
    new DemoEvent("Missing Level", eventStart, eventStart.AddHours(2), basketball.Id, null, locations[ids["you"]]),
    new DemoEvent("Missing Coordinates", eventStart, eventStart.AddHours(2), basketball.Id, 3, locations[ids["no-coordinates"]]),
    new DemoEvent("Touching Times", eventStart.AddHours(2), eventStart.AddHours(3), basketball.Id, 3, locations[ids["you"]]),
    new DemoEvent("Invalid End", eventStart, eventStart.AddHours(-1), basketball.Id, 3, locations[ids["you"]])
};
await using (var transaction = await db.Database.BeginTransactionAsync())
{
    foreach (var item in eventFixtures)
    {
        var name = "Matching Demo Event " + item.Name;
        var entity = await db.Events.SingleOrDefaultAsync(e => e.Name == name);
        if (entity is null) { entity = new Event { Name = name }; db.Events.Add(entity); }
        entity.Date = item.Start;
        entity.EndDate = item.End;
        entity.SkillId = item.SkillId;
        entity.SkillLevel = item.Level;
        entity.Location = item.Location;
        entity.MaxParticipants = 10;
    }
    await db.SaveChangesAsync();
    await transaction.CommitAsync();
}
db.ChangeTracker.Clear();

var repository = new PlayerSkillRepository(db);
var currentUser = new DemoCurrentUser(ids["you"]);
var eventRepository = new EventRepository(db);
var matching = new MatchingService(repository, currentUser, eventRepository, TimeProvider.System);
var matches = await matching.GetMatchesAsync(basketball.Id);
var expectedNames = new[] { "Demo Perfect", "Demo Partial Overlap", "Demo No Overlap", "Demo Missing Availability", "Demo Touching Times Only", "Demo Nearby", "Demo Different Level" };
Check(matches.Select(m => m.Name).SequenceEqual(expectedNames), "Eligible users and ranking");
Check(matches.Select(m => m.Score).SequenceEqual(new[] { 100.0, 100.0, 100.0, 100.0, 100.0, 76.87, 68.75 }), "Player skill/distance scores ignore schedule overlap or missing availability");
Check(matches.All(m => m.UserId != currentUser.Id), "Self exclusion");
Check((await matching.GetMatchesAsync(tennis.Id)).Single().Score == 53.12, "Separate sport levels without availability weighting");
Check((await matching.GetMatchesAsync(int.MaxValue)).Count == 0, "Unplayed sport");
Console.WriteLine($"Basketball skillId={basketball.Id}; tennis skillId={tennis.Id}; demo userId={currentUser.Id}");
foreach (var match in matches) Console.WriteLine($"{match.Name}: {match.Score}");
var feed = await matching.GetFeedAsync(basketball.Id);
var eventMatches = feed.Where(m => m.Type == "event").ToList();
Check(eventMatches.Select(m => m.Name).SequenceEqual(new[] {
    "Matching Demo Event Perfect", "Matching Demo Event Partial Attendance", "Matching Demo Event Overnight", "Matching Demo Event Nearby"
}), "Event eligibility, past/incomplete exclusions, and ordering");
Check(eventMatches.Select(m => m.Score).SequenceEqual(new[] { 100.0, 90.0, 83.33, 81.5 }), "Event scores");
Check(feed.Count == 11 && feed.Select(m => m.Score).SequenceEqual(feed.Select(m => m.Score).OrderDescending()), "Combined player/event order");
Check((await matching.GetFeedAsync(tennis.Id)).Count(m => m.Type == "event") == 1, "Event sport filter");
await using (var transaction = await db.Database.BeginTransactionAsync())
{
    var edge = new Event { Name = "Matching Demo Temporary Week Boundary", Location = locations[ids["you"]], SkillId = basketball.Id, SkillLevel = 3,
        Date = eventStart.AddDays(4), EndDate = eventStart.AddDays(7).AddHours(2) };
    db.Events.Add(edge);
    await db.SaveChangesAsync();
    Check((await matching.GetFeedAsync(basketball.Id)).Single(m => m.Type == "event" && m.Id == edge.Id).Score == 80.54, "Event spanning weekly boundary");
    edge.Date = eventStart;
    edge.EndDate = eventStart.AddDays(7);
    await db.SaveChangesAsync();
    Check((await matching.GetFeedAsync(basketball.Id)).Single(m => m.Type == "event" && m.Id == edge.Id).Score == 80.24, "Full-week event");
    await transaction.RollbackAsync();
}
db.ChangeTracker.Clear();
Console.WriteLine("Event checks passed: scores 100, 90, 83.33, 81.5; combined feed; past/sport/distance/time/missing-data filters; overnight and weekly boundaries.");

var services = new ServiceCollection();
services.AddLogging();
services.AddAutoMapper(c => c.AddProfile<MappingProfile>());
using var provider = services.BuildServiceProvider();
var mapper = provider.GetRequiredService<IMapper>();
mapper.ConfigurationProvider.AssertConfigurationIsValid();
var playerService = new PlayerSkillService(repository, mapper, currentUser);
var userRepository = new UserRepository(db);
var userService = new UserService(userRepository, mapper, hasher);
var ownSkill = (await playerService.GetAllAsync()).Single(p => p.SkillId == basketball.Id);
Check((await userService.GetByIdAsync(currentUser.Id))!.Availabilities.Count == 2, "User availability DTO mapping");
var someoneElse = await db.PlayerSkills.SingleAsync(p => p.UserId == ids["perfect"] && p.SkillId == basketball.Id);
Check(!await playerService.UpdateAsync(someoneElse.Id, new PlayerSkillRequest { SkillId = basketball.Id, SkillLevel = 1 }), "Owner-only schedule updates");
await using (var transaction = await db.Database.BeginTransactionAsync())
{
    Check(await userService.UpdateAvailabilityAsync(currentUser.Id, new()
    {
        new AvailabilityRequest { DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeOnly(18, 0), EndTime = new TimeOnly(19, 0) }
    }), "User availability update");
    Check(await db.Availabilities.CountAsync(a => a.UserId == currentUser.Id) == 1, "Old availability removed on update");
    Check((await matching.GetMatchesAsync(basketball.Id)).Select(m => (m.UserId, m.Score))
        .SequenceEqual(matches.Select(m => (m.UserId, m.Score))), "Changing own schedule preserves basketball candidates and scores");
    Check((await matching.GetMatchesAsync(tennis.Id)).Single().Score == 53.12, "Changing own schedule preserves tennis matching");
    Check(!(await matching.GetFeedAsync(basketball.Id)).Any(m => m.Name == "Matching Demo Event Perfect"), "User availability also filters events");
    Check(await userService.UpdateAvailabilityAsync(currentUser.Id, new()), "Clear own availability");
    Check((await matching.GetMatchesAsync(basketball.Id)).Select(m => (m.UserId, m.Score))
        .SequenceEqual(matches.Select(m => (m.UserId, m.Score))), "Empty own availability preserves player matching");
    var noScheduleFeed = await matching.GetFeedAsync(basketball.Id);
    Check(noScheduleFeed.Count(m => m.Type == "user") == matches.Count && noScheduleFeed.All(m => m.Type != "event"),
        "Without availability, feed still contains players but excludes events");
    Check(await userService.UpdateAvailabilityAsync(currentUser.Id, new()
    {
        new AvailabilityRequest { DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeOnly(18, 0), EndTime = new TimeOnly(19, 0) }
    }), "Restore schedule for sport-deletion check");
    Check(await playerService.DeleteAsync(ownSkill.Id), "Delete own sport");
    Check(await db.Availabilities.CountAsync(a => a.UserId == currentUser.Id) == 1, "Deleting sport preserves user availability");
    await transaction.RollbackAsync();
}
db.ChangeTracker.Clear();
Console.WriteLine("Database checks passed: player scores independent of both schedules, distance limits, event availability, per-sport skills, JWT owner, availability replacement.");

if (args.Length > 0)
{
    using var http = new HttpClient { BaseAddress = new Uri(args[0]) };
    var anonymous = await http.GetAsync($"api/Matches?skillId={basketball.Id}");
    Check(anonymous.StatusCode == HttpStatusCode.Unauthorized, "Matches requires JWT");
    Check((await http.GetAsync($"api/Matches/feed?skillId={basketball.Id}")).StatusCode == HttpStatusCode.Unauthorized, "Combined feed requires JWT");
    var login = await http.PostAsJsonAsync("api/Auth/login", new LoginRequest { Email = "matching-demo.you@example.test", Password = demoPassword });
    login.EnsureSuccessStatusCode();
    var auth = (await login.Content.ReadFromJsonAsync<AuthResponse>())!;
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
    var response = await http.GetFromJsonAsync<List<MatchDto>>($"api/Matches?skillId={basketball.Id}&userId={ids["perfect"]}");
    Check(response!.Select(m => m.Name).SequenceEqual(expectedNames), "GET endpoint uses JWT, not supplied userId");
    Check(response!.Select(m => m.Score).SequenceEqual(matches.Select(m => m.Score)), "HTTP scores");
    var feedResponse = await http.GetFromJsonAsync<List<FeedMatchDto>>($"api/Matches/feed?skillId={basketball.Id}&userId={ids["perfect"]}");
    Check(feedResponse!.Select(m => (m.Type, m.Name, m.Score)).SequenceEqual(feed.Select(m => (m.Type, m.Name, m.Score))), "HTTP combined feed and JWT identity");
    var eventRequest = new EventRequest { Name = "Matching Demo HTTP Event", Date = eventStart, EndDate = eventStart.AddHours(2),
        SkillId = basketball.Id, SkillLevel = 3, Location = locations[ids["you"]], MaxParticipants = 1 };
    var createdResponse = await http.PostAsJsonAsync("api/Events", eventRequest);
    createdResponse.EnsureSuccessStatusCode();
    var createdEvent = (await createdResponse.Content.ReadFromJsonAsync<EventDto>())!;
    UserEventDto? registration = null;
    try
    {
        Check(createdEvent.SkillId == basketball.Id && createdEvent.EndDate == eventRequest.EndDate, "Event creation DTO");
        Check((await http.GetFromJsonAsync<List<FeedMatchDto>>($"api/Matches/feed?skillId={basketball.Id}"))!
            .Single(m => m.Type == "event" && m.Id == createdEvent.Id).Score == 100, "Created event appears scored in feed");
        var joinedResponse = await http.PostAsJsonAsync("api/UserEvents", new { eventId = createdEvent.Id, userId = ids["perfect"] });
        joinedResponse.EnsureSuccessStatusCode();
        registration = (await joinedResponse.Content.ReadFromJsonAsync<UserEventDto>())!;
        Check(registration.UserId == currentUser.Id && registration.EventId == createdEvent.Id, "Join uses JWT, not supplied user ID");
        Check(!(await http.GetFromJsonAsync<List<FeedMatchDto>>($"api/Matches/feed?skillId={basketball.Id}"))!
            .Any(m => m.Type == "event" && m.Id == createdEvent.Id), "Joined event is excluded from own feed");
        var duplicateResponse = await http.PostAsJsonAsync("api/UserEvents", new UserEventRequest { EventId = createdEvent.Id });
        duplicateResponse.EnsureSuccessStatusCode();
        Check((await duplicateResponse.Content.ReadFromJsonAsync<UserEventDto>())!.Id == registration.Id, "Duplicate join returns existing registration");
        Check((await http.GetFromJsonAsync<List<UserEventDto>>("api/UserEvents"))!.Count(r => r.EventId == createdEvent.Id) == 1, "Joined state can be reloaded");
        var joinedEvents = (await http.GetFromJsonAsync<List<EventDto>>("api/Events/joined"))!;
        var myRegistrations = (await http.GetFromJsonAsync<List<UserEventDto>>("api/UserEvents"))!;
        Check(joinedEvents.Select(e => e.Id).Order().SequenceEqual(myRegistrations.Select(r => r.EventId).Order()), "Joined-events endpoint contains only own joined events");
        Check(joinedEvents.Single(e => e.Id == createdEvent.Id).ParticipantCount == 1, "Participant count");
        using var other = new HttpClient { BaseAddress = http.BaseAddress };
        var otherLogin = await other.PostAsJsonAsync("api/Auth/login", new LoginRequest { Email = "matching-demo.perfect@example.test", Password = demoPassword });
        otherLogin.EnsureSuccessStatusCode();
        other.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await otherLogin.Content.ReadFromJsonAsync<AuthResponse>())!.AccessToken);
        Check((await other.DeleteAsync($"api/UserEvents/{registration.Id}")).StatusCode == HttpStatusCode.NotFound, "Cannot leave another user's registration");
        Check((await other.PostAsJsonAsync("api/UserEvents", new UserEventRequest { EventId = createdEvent.Id })).StatusCode == HttpStatusCode.Conflict, "Full event rejects another participant");
        Check((await http.DeleteAsync($"api/UserEvents/{registration.Id}")).StatusCode == HttpStatusCode.NoContent, "Leave own event");
        Check(!(await http.GetFromJsonAsync<List<UserEventDto>>("api/UserEvents"))!.Any(r => r.EventId == createdEvent.Id), "Left state can be reloaded");
        Check(!(await http.GetFromJsonAsync<List<EventDto>>("api/Events/joined"))!.Any(e => e.Id == createdEvent.Id), "Leaving removes event from own joined list");
        Check((await http.GetFromJsonAsync<List<FeedMatchDto>>($"api/Matches/feed?skillId={basketball.Id}"))!
            .Any(m => m.Type == "event" && m.Id == createdEvent.Id), "Leaving restores eligible event to feed");
        try
        {
            var competingJoins = await Task.WhenAll(
                http.PostAsJsonAsync("api/UserEvents", new UserEventRequest { EventId = createdEvent.Id }),
                other.PostAsJsonAsync("api/UserEvents", new UserEventRequest { EventId = createdEvent.Id }));
            Check(competingJoins.Count(r => r.StatusCode == HttpStatusCode.Created) == 1
                && competingJoins.Count(r => r.StatusCode == HttpStatusCode.Conflict) == 1, "Concurrent joins cannot overbook a one-place event");
            Check((await http.GetFromJsonAsync<EventDto>($"api/Events/{createdEvent.Id}"))!.ParticipantCount == 1, "Capacity remains one after concurrent joins");
        }
        finally
        {
            foreach (var client in new[] { http, other })
                foreach (var row in (await client.GetFromJsonAsync<List<UserEventDto>>("api/UserEvents"))!.Where(r => r.EventId == createdEvent.Id))
                    (await client.DeleteAsync($"api/UserEvents/{row.Id}")).EnsureSuccessStatusCode();
        }
        Console.WriteLine("Capacity checks passed: joined-only list, counts, full events, concurrent joins and leaving frees capacity.");
        Console.WriteLine("Attendance checks passed: JWT identity, join, repeated join, reload, leave, and cross-user protection.");
    }
    finally
    {
        if (registration is not null) await http.DeleteAsync($"api/UserEvents/{registration.Id}");
        (await http.DeleteAsync($"api/Events/{createdEvent.Id}")).EnsureSuccessStatusCode();
    }
    eventRequest.EndDate = eventRequest.Date;
    Check((await http.PostAsJsonAsync("api/Events", eventRequest)).StatusCode == HttpStatusCode.BadRequest, "Invalid event duration rejected");
    var invalid = await http.GetAsync("api/Matches?skillId=0");
    Check(invalid.StatusCode == HttpStatusCode.BadRequest, "Invalid sport ID");
    var scheduleResponse = await http.GetFromJsonAsync<UserDto>("api/Users/me");
    Check(scheduleResponse!.Availabilities.Count == 2, "HTTP user availability read");
    var validSchedule = new List<AvailabilityRequest>
    {
        new AvailabilityRequest { DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeOnly(18, 0), EndTime = new TimeOnly(20, 0) },
        new AvailabilityRequest { DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeOnly(18, 30), EndTime = new TimeOnly(19, 30) }
    };
    var updated = await http.PutAsJsonAsync($"api/Users/me/availability?userId={ids["perfect"]}", validSchedule);
    Check(updated.StatusCode == HttpStatusCode.NoContent, "HTTP availability update");
    db.ChangeTracker.Clear();
    Check(await db.Availabilities.CountAsync(a => a.UserId == ids["perfect"]) == 1, "Availability endpoint ignores spoofed user ID");
    validSchedule[0].EndTime = new TimeOnly(17, 0);
    var invalidSchedule = await http.PutAsJsonAsync("api/Users/me/availability", validSchedule);
    Check(invalidSchedule.StatusCode == HttpStatusCode.BadRequest, "Invalid time slot rejected");
    Console.WriteLine("HTTP checks passed: login, protected GET, score ordering, token user ID, validation, availability.");
}
Console.WriteLine("Demo login: matching-demo.you@example.test / MatchDemo-2026!");

async Task<Skill> Sport(string name)
{
    var sport = await db.Skills.SingleOrDefaultAsync(s => s.Name == name);
    if (sport is null) { sport = new Skill { Name = name }; db.Skills.Add(sport); await db.SaveChangesAsync(); }
    return sport;
}
async Task SetPlayerSkill(int userId, int skillId, int level)
{
    var player = await db.PlayerSkills.SingleOrDefaultAsync(p => p.UserId == userId && p.SkillId == skillId);
    if (player is null) { player = new PlayerSkill { UserId = userId, SkillId = skillId }; db.PlayerSkills.Add(player); }
    player.SkillLevel = level;
    await db.SaveChangesAsync();
}
static Availability Slot(int day, int start, int end) => new() { DayOfWeek = (DayOfWeek)day, StartTime = new TimeOnly(start, 0), EndTime = new TimeOnly(end, 0) };
static void Check(bool condition, string message) { if (!condition) throw new Exception("Failed: " + message); }
sealed record Fixture(string Key, string Name, int Level, int? Range, string? Latitude, int Day, int Start, int End, bool Tennis = false);
sealed record DemoEvent(string Name, DateTime Start, DateTime? End, int? SkillId, int? Level, int Location);
sealed class DemoCurrentUser(int id) : ICurrentUser { public int Id => id; }
