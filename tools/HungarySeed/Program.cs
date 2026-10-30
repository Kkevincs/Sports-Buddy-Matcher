using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.Repository;
using SportsBuddyMatcher.Repository.Models;

// Run from the repository root. Only missing reserved demo emails and sport names are inserted.
var apiRoot = Path.Combine(Directory.GetCurrentDirectory(), "SportsBuddyMatcher", "SportsBuddyMatcher");
DotNetEnv.Env.NoClobber().Load(Path.Combine(apiRoot, ".env"));
var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(apiRoot, "appsettings.json")).AddEnvironmentVariables().Build();
var options = new DbContextOptionsBuilder<SportsBuddyMatcherDbContext>()
    .UseSqlServer(config.GetConnectionString("DefaultConnection")).Options;
await using var db = new SportsBuddyMatcherDbContext(options);
const string emailDomain = "@hungary-seed.example.test";
const string password = "HungaryDemo-2026!";
const int usersPerCity = 15;

var sportNames = new[]
{
    "Soccer", "Basketball", "Tennis", "Table tennis", "Badminton", "Volleyball",
    "Beach volleyball", "Handball", "Futsal", "Running", "Trail running", "Walking",
    "Hiking", "Cycling", "Mountain biking", "Road cycling", "Swimming", "Water polo",
    "Kayaking", "Canoeing", "Rowing", "Stand-up paddleboarding", "Triathlon", "Athletics",
    "Fitness", "Weightlifting", "Powerlifting", "CrossFit", "Calisthenics", "Yoga",
    "Pilates", "Boxing", "Kickboxing", "Judo", "Karate", "Taekwondo", "Wrestling",
    "Fencing", "Squash", "Padel", "Bowling", "Darts", "Archery", "Golf",
    "Ultimate frisbee", "Rugby", "American football", "Ice hockey", "Ice skating",
    "Roller skating", "Skateboarding", "Climbing", "Bouldering", "Horse riding",
    "Orienteering", "Sailing", "Windsurfing", "Table football", "Disc golf", "Baseball"
};
var cities = new[]
{
    new City("Budapest", "budapest", "1051"), new City("Debrecen", "debrecen", "4024"),
    new City("Szeged", "szeged", "6720"), new City("Miskolc", "miskolc", "3525"),
    new City("Pécs", "pecs", "7621"), new City("Győr", "gyor", "9021"),
    new City("Nyíregyháza", "nyiregyhaza", "4400"), new City("Kecskemét", "kecskemet", "6000"),
    new City("Székesfehérvár", "szekesfehervar", "8000"), new City("Szombathely", "szombathely", "9700"),
    new City("Szolnok", "szolnok", "5000"), new City("Tatabánya", "tatabanya", "2800"),
    new City("Veszprém", "veszprem", "8200"), new City("Kaposvár", "kaposvar", "7400"),
    new City("Békéscsaba", "bekescsaba", "5600"), new City("Zalaegerszeg", "zalaegerszeg", "8900"),
    new City("Eger", "eger", "3300"), new City("Sopron", "sopron", "9400"),
    new City("Nagykanizsa", "nagykanizsa", "8800"), new City("Dunaújváros", "dunaujvaros", "2400"),
    new City("Hódmezővásárhely", "hodmezovasarhely", "6800"), new City("Salgótarján", "salgotarjan", "3100"),
    new City("Cegléd", "cegled", "2700"), new City("Baja", "baja", "6500"),
    new City("Szentendre", "szentendre", "2000"), new City("Esztergom", "esztergom", "2500"),
    new City("Gyula", "gyula", "5700"), new City("Hévíz", "heviz", "8380"),
    new City("Siófok", "siofok", "8600"), new City("Balatonfüred", "balatonfured", "8230")
};
string Email(City city, int number) => $"{city.Slug}.{number:00}{emailDomain}";
var existingEmails = (await db.Users.Where(u => u.Email.EndsWith(emailDomain)).Select(u => u.Email).ToListAsync())
    .ToHashSet(StringComparer.OrdinalIgnoreCase);
var centers = new Dictionary<string, (double Latitude, double Longitude)>();
using var geocoding = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
var apiKey = config["GEOAPIFY_API_KEY"] ?? throw new InvalidOperationException("GEOAPIFY_API_KEY is missing.");
foreach (var city in cities.Where(c => Enumerable.Range(1, usersPerCity).Any(n => !existingEmails.Contains(Email(c, n)))))
{
    var url = $"https://api.geoapify.com/v1/geocode/search?country=Hungary&city={Uri.EscapeDataString(city.Name)}&format=json&limit=1&bias=countrycode:none&apiKey={Uri.EscapeDataString(apiKey)}";
    using var response = await geocoding.GetAsync(url);
    response.EnsureSuccessStatusCode();
    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    var results = json.RootElement.GetProperty("results");
    if (results.GetArrayLength() == 0) throw new InvalidOperationException($"Could not locate {city.Name}.");
    centers[city.Slug] = (results[0].GetProperty("lat").GetDouble(), results[0].GetProperty("lon").GetDouble());
    Console.WriteLine($"Located {city.Name}.");
}

var firstNames = new[] { "Ádám", "Anna", "Bence", "Eszter", "Dániel", "Zsófia", "Máté", "Réka", "Péter", "Lilla", "Gábor", "Nóra", "Balázs", "Júlia", "Tamás", "Dóra", "Márk", "Petra", "András", "Kata" };
var lastNames = new[] { "Nagy", "Kovács", "Tóth", "Szabó", "Horváth", "Varga", "Kiss", "Molnár", "Németh", "Farkas", "Balogh", "Papp", "Lakatos", "Takács", "Juhász", "Mészáros", "Oláh", "Simon", "Rácz", "Fekete" };
var ranges = new[] { 5, 10, 15, 20, 30, 50 };
var hasher = new PasswordHasher<User>();
int addedUsers = 0, addedSports = 0;
await using (var transaction = await db.Database.BeginTransactionAsync())
{
    var sports = (await db.Skills.ToListAsync()).ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
    foreach (var name in sportNames)
    {
        if (sports.ContainsKey(name)) continue;
        var sport = new Skill { Name = name };
        db.Skills.Add(sport);
        sports.Add(name, sport);
        addedSports++;
    }
    await db.SaveChangesAsync();
    for (var cityIndex = 0; cityIndex < cities.Length; cityIndex++)
    {
        var city = cities[cityIndex];
        var cityAdded = 0;
        for (var number = 1; number <= usersPerCity; number++)
        {
            var email = Email(city, number);
            if (existingEmails.Contains(email)) continue;
            var index = cityIndex * usersPerCity + number - 1;
            var random = new Random(20261028 + index);
            var center = centers[city.Slug];
            // Demo addresses are fictional. Coordinates are representative points within 3 km of the city centre.
            var radius = Math.Sqrt(random.NextDouble()) * 3;
            var angle = random.NextDouble() * 2 * Math.PI;
            var latitude = center.Latitude + radius * Math.Sin(angle) / 111.195;
            var longitude = center.Longitude + radius * Math.Cos(angle) / (111.195 * Math.Cos(center.Latitude * Math.PI / 180));
            var user = new User
            {
                Name = $"{lastNames[index % lastNames.Length]} {firstNames[(index / lastNames.Length + index) % firstNames.Length]} (Demo)",
                Email = email, PasswordHash = "", PreferredRangeKm = ranges[random.Next(ranges.Length)],
                LocationNavigation = new Location
                {
                    Country = "Hungary", City = city.Name, Street = $"Minta utca {number}",
                    Postcode = city.Postcode, Latitude = latitude.ToString("R", CultureInfo.InvariantCulture),
                    Longitude = longitude.ToString("R", CultureInfo.InvariantCulture)
                },
                Availabilities = new List<Availability>
                {
                    Slot(DayOfWeek.Tuesday, 18, 20),
                    Slot(number % 2 == 0 ? DayOfWeek.Saturday : DayOfWeek.Sunday, 9, 12),
                    Slot(number % 3 == 0 ? DayOfWeek.Wednesday : DayOfWeek.Thursday, 17 + random.Next(2), 21)
                }
            };
            user.PasswordHash = hasher.HashPassword(user, password);
            var selected = new HashSet<string> { "Soccer", "Basketball", "Running", sportNames[index % sportNames.Length] };
            var count = random.Next(4, 7);
            while (selected.Count < count) selected.Add(sportNames[random.Next(sportNames.Length)]);
            foreach (var name in selected)
                user.PlayerSkills.Add(new PlayerSkill { SkillId = sports[name].Id, SkillLevel = random.Next(1, 6) });
            db.Users.Add(user);
            addedUsers++;
            cityAdded++;
        }
        await db.SaveChangesAsync();
        if (cityAdded > 0) Console.WriteLine($"{city.Name}: added {cityAdded} demo users.");
    }
    await transaction.CommitAsync();
}
db.ChangeTracker.Clear();
var users = db.Users.Where(u => u.Email.EndsWith(emailDomain));
var userCount = await users.CountAsync();
var cityCount = await users.Select(u => u.LocationNavigation.City).Distinct().CountAsync();
var sportCount = await db.Skills.CountAsync(s => sportNames.Contains(s.Name));
var playerSkills = await db.PlayerSkills.CountAsync(p => p.User.Email.EndsWith(emailDomain));
var availabilityCount = await db.Availabilities.CountAsync(a => a.User.Email.EndsWith(emailDomain));
if (userCount != 450 || cityCount != 30 || sportCount != 60) throw new Exception("Seed coverage did not match the expected totals.");
if (await users.AnyAsync(u => u.Availabilities.Count != 3 || u.PlayerSkills.Count < 4 || u.PlayerSkills.Count > 6))
    throw new Exception("A seeded user has incomplete sports or availability.");
Console.WriteLine($"Created {addedUsers} users and {addedSports} new sport names.");
Console.WriteLine($"Verified: {userCount} Hungarian demo users, {cityCount} cities, {sportCount} catalog sports, {playerSkills} player-skill records, {availabilityCount} weekly time slots.");
Console.WriteLine("Sample login: budapest.01@hungary-seed.example.test / HungaryDemo-2026!");
Console.WriteLine("Other cities: use the city slug and .01 through .15 with @hungary-seed.example.test.");

if (args.Length > 0)
{
    using var http = new HttpClient { BaseAddress = new Uri(args[0]) };
    var sportId = await db.Skills.Where(s => s.Name == "Basketball").Select(s => s.Id).SingleAsync();
    foreach (var slug in new[] { "budapest", "debrecen", "gyor" })
    {
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        var login = await http.PostAsJsonAsync("api/Auth/login", new LoginRequest { Email = $"{slug}.01{emailDomain}", Password = password });
        login.EnsureSuccessStatusCode();
        var auth = (await login.Content.ReadFromJsonAsync<AuthResponse>())!;
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var profile = (await http.GetFromJsonAsync<UserDto>("api/Users/me"))!;
        var matches = (await http.GetFromJsonAsync<List<MatchDto>>($"api/Matches?skillId={sportId}"))!;
        if (matches.Count == 0 || matches.Any(m => m.UserId == profile.Id)
            || !matches.Select(m => m.Score).SequenceEqual(matches.Select(m => m.Score).OrderDescending()))
            throw new Exception($"The seeded {slug} account did not produce a valid ranked match list.");
        var feed = (await http.GetFromJsonAsync<List<FeedMatchDto>>($"api/Matches/feed?skillId={sportId}"))!;
        if (feed.Count(m => m.Type == "user") != matches.Count)
            throw new Exception("The match feed did not include all ranked users.");
        Console.WriteLine($"HTTP verified: {slug} login and {matches.Count} ranked basketball matches in the feed ({elapsed.ElapsedMilliseconds} ms).");
    }
}

static Availability Slot(DayOfWeek day, int start, int end) => new() { DayOfWeek = day, StartTime = new TimeOnly(start, 0), EndTime = new TimeOnly(end, 0) };
sealed record City(string Name, string Slug, string Postcode);
