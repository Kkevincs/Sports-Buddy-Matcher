using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SportsBuddyMatcher.Repository;
using SportsBuddyMatcher.Repository.Models;

// Fictional development data. Reserved email addresses identify this seed without labels in display names.
var root = Directory.GetCurrentDirectory();
var apiRoot = Path.Combine(root, "SportsBuddyMatcher", "SportsBuddyMatcher");
var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(apiRoot, "appsettings.json")).Build();
await using var db = new SportsBuddyMatcherDbContext(new DbContextOptionsBuilder<SportsBuddyMatcherDbContext>()
    .UseSqlServer(config.GetConnectionString("DefaultConnection")).Options);
const string domain = "@netherlands-seed.example.test";
const string password = "Netherlands-2026!";
var cities = new[] {
    new City("Amsterdam", "amsterdam", "1012 JS", 52.3676, 4.9041),
    new City("Rotterdam", "rotterdam", "3011 AA", 51.9244, 4.4777),
    new City("The Hague", "the-hague", "2511 BT", 52.0705, 4.3007),
    new City("Utrecht", "utrecht", "3511 CE", 52.0907, 5.1214),
    new City("Eindhoven", "eindhoven", "5611 EM", 51.4416, 5.4697),
    new City("Groningen", "groningen", "9711 LV", 53.2194, 6.5665),
    new City("Tilburg", "tilburg", "5038 CB", 51.5555, 5.0913),
    new City("Almere", "almere", "1315 JS", 52.3508, 5.2647),
    new City("Breda", "breda", "4811 DJ", 51.5719, 4.7683),
    new City("Nijmegen", "nijmegen", "6511 PS", 51.8426, 5.8528)
};
var firstNames = new[] { "Jeroen", "Sanne", "Omar", "Lotte", "Rik", "Noor", "Daan", "Eva", "Youssef", "Femke", "Bram", "Lieke", "Lucas", "Sophie", "Amir", "Julia", "Thijs", "Emma", "Sem", "Sara" };
var lastNames = new[] { "de Vries", "Jansen", "Bakker", "Visser", "Smit", "Meijer", "de Boer", "Mulder", "de Groot", "Bos", "Vos", "Peters", "Hendriks", "van Dijk", "van Leeuwen", "Dekker", "Brouwer", "Dijkstra", "Willems", "van den Berg" };
var bios = new[] {
    "Always up for a friendly game after work. I enjoy meeting people and improving together.",
    "Looking for a regular sports buddy nearby. Weeknight sessions and relaxed weekend games suit me best.",
    "Getting back into sport and enjoying every session. Happy to play with people at different levels.",
    "A little competition, plenty of encouragement, and a coffee afterwards sounds like a good evening.",
    "I like staying active outdoors. Looking for a small group to train with regularly.",
    "New partners are welcome! I usually have time in the evenings and at the weekend."
};
var sports = new[] { "Soccer", "Basketball", "Tennis", "Running", "Cycling" };
var titles = new[] { "Five-a-side football", "Basketball open run", "Friendly tennis doubles", "Social park run", "Evening cycling loop" };
var descriptions = new[] {
    "A friendly five-a-side session with rotating teams. Bring boots, water, and a light and dark shirt.",
    "A relaxed pickup game with balanced teams. Warm up together, then play short games and rotate.",
    "Casual doubles with rotating partners. Bring your racket; we will start with a gentle warm-up.",
    "An easy-paced group run of around 5 km. We stay together and finish with a stretch and a chat.",
    "A social ride of around 20 km at a comfortable pace. Bring a helmet, lights, water, and a working bike."
};
foreach (var sport in sports)
    if (!await db.Skills.AnyAsync(s => s.Name == sport)) db.Skills.Add(new Skill { Name = sport });
await db.SaveChangesAsync();
var skillIds = await db.Skills.Where(s => sports.Contains(s.Name)).ToDictionaryAsync(s => s.Name, s => s.Id);
var hasher = new PasswordHasher<User>();
var firstDay = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Europe/Budapest").Date, DateTimeKind.Unspecified);
var manifest = new List<object>();
int addedUsers = 0, addedEvents = 0;
await using var transaction = await db.Database.BeginTransactionAsync();
for (int c = 0; c < cities.Length; c++)
{
    var city = cities[c];
    var cityUsers = new List<User>();
    for (int n = 0; n < 12; n++)
    {
        var email = $"{city.Slug}.{n + 1:00}{domain}";
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            var index = c * 12 + n;
            var random = new Random(20261028 + index);
            user = new User {
                Name = $"{firstNames[index % firstNames.Length]} {lastNames[(index / firstNames.Length + n * 3) % lastNames.Length]}",
                Email = email, PasswordHash = "", PreferredRangeKm = new[] { 10m, 15m, 20m, 30m }[n % 4],
                Bio = bios[index % bios.Length], ProfilePictureUrl = null,
                LocationNavigation = Address(city, "Parklaan " + (n + 1), (random.NextDouble() - .5) * .018, (random.NextDouble() - .5) * .025)
            };
            user.PasswordHash = hasher.HashPassword(user, password);
            foreach (var sport in sports) user.PlayerSkills.Add(new PlayerSkill { SkillId = skillIds[sport], SkillLevel = 2 + random.Next(4) });
            foreach (var day in Enum.GetValues<DayOfWeek>())
                user.Availabilities.Add(new Availability { DayOfWeek = day, StartTime = new TimeOnly(day is DayOfWeek.Saturday or DayOfWeek.Sunday ? 8 : 17, 0), EndTime = new TimeOnly(22, 0) });
            db.Users.Add(user);
            addedUsers++;
        }
        cityUsers.Add(user);
        manifest.Add(new { user.Name, user.Email, City = city.Name });
    }
    await db.SaveChangesAsync();
    for (int s = 0; s < sports.Length; s++)
    {
        var owner = cityUsers[s];
        var name = $"{titles[s]} · {city.Name}";
        if (await db.Events.AnyAsync(e => e.CreatedById == owner.Id && e.Name == name)) continue;
        var asset = $"/images/netherlands/{sports[s].ToLowerInvariant()}.png";
        if (!File.Exists(Path.Combine(apiRoot, "wwwroot", asset.TrimStart('/')))) throw new Exception("Missing generated event photo: " + asset);
        var starts = firstDay.AddDays(2 + s + c % 4).AddHours(18);
        var item = new Event {
            Name = name, Bio = descriptions[s], SkillId = skillIds[sports[s]], SkillLevel = 3,
            Date = starts, EndDate = starts.AddHours(s == 4 ? 2 : 1.5), MaxParticipants = new[] { 10, 12, 4, 20, 16 }[s],
            CreatedById = owner.Id, PictureUrl = asset,
            LocationNavigation = Address(city, s == 2 ? "Tennispark 1" : "Stadspark 1", .002, .002)
        };
        // The first account in each city remains unjoined so it can see every local event in its feed.
        foreach (var member in cityUsers.Skip(7).Take(s == 2 ? 2 : 3)) item.UserEvents.Add(new UserEvent { UserId = member.Id });
        db.Events.Add(item);
        addedEvents++;
    }
    await db.SaveChangesAsync();
    Console.WriteLine($"{city.Name}: 12 accounts; 5 sports events.");
}
await transaction.CommitAsync();
var seeded = db.Users.Where(u => u.Email.EndsWith(domain));
var userCount = await seeded.CountAsync();
var eventCount = await db.Events.CountAsync(e => e.CreatedBy != null && e.CreatedBy.Email.EndsWith(domain));
if (userCount != 120 || eventCount != 50 || await seeded.AnyAsync(u => u.ProfilePictureUrl != null || u.Name.Contains("Demo")))
    throw new Exception("Seed totals or profile requirements were not met.");
await File.WriteAllTextAsync(Path.Combine(root, "tools", "NetherlandsSeed", "accounts.json"), JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Added {addedUsers} accounts, {addedEvents} events. Verified {userCount} accounts, {eventCount} events across 10 cities, with no profile photos.");
Console.WriteLine($"Login: eindhoven.01{domain} / {password}");

static Location Address(City city, string street, double latOffset, double lonOffset) => new() {
    Country = "Netherlands", City = city.Name, Street = street, Postcode = city.Postcode,
    Latitude = (city.Latitude + latOffset).ToString("R", CultureInfo.InvariantCulture),
    Longitude = (city.Longitude + lonOffset).ToString("R", CultureInfo.InvariantCulture)
};
record City(string Name, string Slug, string Postcode, double Latitude, double Longitude);
