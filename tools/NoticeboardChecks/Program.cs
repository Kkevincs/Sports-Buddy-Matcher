using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SportsBuddyMatcher.BusinessLogic.DTOs;

var baseUrl = args.Length > 0 ? args[0] : "http://127.0.0.1:5199/";
HttpClient Client() => new(new HttpClientHandler { UseProxy = false }) { BaseAddress = new Uri(baseUrl) };
using var demo = Client();
await Login(demo, "matching-demo.you@example.test", "MatchDemo-2026!");
var home = (await demo.GetFromJsonAsync<UserDto>("api/Users/me"))!.Location;
var sportId = (await demo.GetFromJsonAsync<List<SkillDto>>("api/Skills"))!.First(s => s.Name == "Basketball").Id;
var clients = new List<HttpClient>();
var users = new List<UserDto>();
var connectionIds = new HashSet<int>();
var uploaded = new List<string>();
int? eventId = null;
try
{
    foreach (var label in new[] { "A", "B", "C" })
    {
        var client = Client(); clients.Add(client);
        var registration = await client.PostAsJsonAsync("api/Auth/register", new UserRequest {
            Name = "Noticeboard Test " + label, Email = "noticeboard-" + Guid.NewGuid().ToString("N") + "@example.test",
            Password = "Noticeboard-Test-2026!", Location = home, PreferredRangeKm = 7.5m });
        registration.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await registration.Content.ReadFromJsonAsync<AuthResponse>())!.AccessToken);
        users.Add((await client.GetFromJsonAsync<UserDto>("api/Users/me"))!);
    }
    var a=clients[0]; var b=clients[1]; var c=clients[2];
    (await a.PatchAsJsonAsync("api/Users/me/profile", new ProfileRequest { Name=users[0].Name, Bio="I love basketball and running. <script>not markup</script>" })).EnsureSuccessStatusCode();
    (await a.PatchAsJsonAsync("api/Users/me/preferences", new PreferencesRequest { PreferredRangeKm=12.75m })).EnsureSuccessStatusCode();
    Check((await a.GetFromJsonAsync<UserDto>("api/Users/me"))!.PreferredRangeKm==12.75m,"Decimal kilometre range");
    var profileResponse=await b.GetAsync("api/Users/"+users[0].Id); profileResponse.EnsureSuccessStatusCode();
    using(var json=JsonDocument.Parse(await profileResponse.Content.ReadAsStringAsync())) {
        Check(json.RootElement.GetProperty("bio").GetString()!.Contains("basketball"),"Public bio");
        foreach(var field in new[]{"email","passwordHash","location","latitude","longitude"})
            Check(!json.RootElement.TryGetProperty(field,out _),"Public profile must not expose "+field);
    }
    var request=await Post<ConnectionDto>(a,"api/Connections",new ConnectionRequest {TargetUserId=users[1].Id}); connectionIds.Add(request.Id);
    Check(request.Status=="pending"&&!request.IsIncoming,"Outgoing request");
    Check((await b.GetFromJsonAsync<List<ConnectionDto>>("api/Connections"))!.Single().IsIncoming,"Incoming request");
    Check((await Post<ConnectionDto>(b,"api/Connections",new ConnectionRequest {TargetUserId=users[0].Id})).Id==request.Id,"Reverse request does not duplicate pair");
    Check((await a.PostAsync("api/Connections/"+request.Id+"/accept",null)).StatusCode==HttpStatusCode.Forbidden,"Requester cannot accept");
    Check((await c.DeleteAsync("api/Connections/"+request.Id)).StatusCode==HttpStatusCode.NotFound,"Unrelated user cannot remove");
    (await b.PostAsync("api/Connections/"+request.Id+"/accept",null)).EnsureSuccessStatusCode();
    Check((await a.GetFromJsonAsync<List<ConnectionDto>>("api/Connections"))!.Single().Status=="connected","Connection accepted");
    Check((await a.PostAsJsonAsync("api/Connections",new ConnectionRequest{TargetUserId=users[0].Id})).StatusCode==HttpStatusCode.BadRequest,"Self connection rejected");
    (await b.DeleteAsync("api/Connections/"+request.Id)).EnsureSuccessStatusCode();
    Check((await a.GetFromJsonAsync<List<ConnectionDto>>("api/Connections"))!.Count==0,"Disconnect removes pair");
    var photoPath=Path.Combine(Directory.GetCurrentDirectory(),"SportsBuddyMatcher/SportsBuddyMatcher/wwwroot/images/basketball.jpg");
    var bytes=await File.ReadAllBytesAsync(photoPath);
    using(var upload=Photo(bytes,"photo.jpg")) {
        var response=await a.PostAsync("api/Users/me/photo",upload); response.EnsureSuccessStatusCode();
        var result=await response.Content.ReadFromJsonAsync<JsonElement>(); uploaded.Add(result.GetProperty("profilePictureUrl").GetString()!);
    }
    Check((await b.GetFromJsonAsync<PublicProfileDto>("api/Users/"+users[0].Id))!.ProfilePictureUrl==uploaded[0],"Uploaded public profile photo");
    using(var upload=Photo(System.Text.Encoding.UTF8.GetBytes("<svg><script>alert(1)</script></svg>"),"fake.jpg"))
        Check((await a.PostAsync("api/Users/me/photo",upload)).StatusCode==HttpStatusCode.BadRequest,"SVG disguised as JPEG rejected");
    using(var publicClient=Client()) {
        var file=await publicClient.GetAsync(uploaded[0]);
        Check(file.StatusCode==HttpStatusCode.OK && file.Content.Headers.ContentType!.MediaType=="image/jpeg","Photo is served as an image");
        Check(file.Headers.GetValues("X-Content-Type-Options").Single()=="nosniff","Photo content sniffing blocked");
    }
    var eventRequest=new EventRequest{Name="Noticeboard Temporary Event",Bio="A relaxed game for everyone.",SkillId=sportId,SkillLevel=3,Location=home,MaxParticipants=5,
        Date=DateTime.Now.AddDays(7),EndDate=DateTime.Now.AddDays(7).AddHours(2)};
    var created=await Post<EventDto>(a,"api/Events",eventRequest); eventId=created.Id;
    Check(created.CreatedById==users[0].Id && created.Bio==eventRequest.Bio,"Event bio and creator");
    using(var upload=Photo(bytes,"event.jpg")) {
        var response=await a.PostAsync("api/Events/"+eventId+"/photo",upload); response.EnsureSuccessStatusCode();
        uploaded.Add((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("pictureUrl").GetString()!);
    }
    using(var upload=Photo(bytes,"event.jpg"))
        Check((await b.PostAsync("api/Events/"+eventId+"/photo",upload)).StatusCode==HttpStatusCode.Forbidden,"Only owner can change event photo");
    Check((await b.PutAsJsonAsync("api/Events/"+eventId,eventRequest)).StatusCode==HttpStatusCode.Forbidden,"Only owner can edit event");
    Check((await b.DeleteAsync("api/Events/"+eventId)).StatusCode==HttpStatusCode.Forbidden,"Only owner can delete event");
    eventRequest.Bio="Updated session bio.";
    (await a.PutAsJsonAsync("api/Events/"+eventId,eventRequest)).EnsureSuccessStatusCode();
    var updated=(await a.GetFromJsonAsync<EventDto>("api/Events/"+eventId))!;
    Check(updated.Bio==eventRequest.Bio && updated.PictureUrl==uploaded[1] && updated.CreatedById==users[0].Id,"Editing preserves photo and ownership");
    Console.WriteLine("Noticeboard checks passed: private/public profiles, editable bios, decimal range, bidirectional requests, accept/decline ownership, no duplicate pairs, photo upload/validation, static image headers, and event ownership.");
}
finally
{
    if(clients.Count>0) {
        foreach(var id in connectionIds) await clients[0].DeleteAsync("api/Connections/"+id);
        if(eventId.HasValue) await clients[0].DeleteAsync("api/Events/"+eventId);
        foreach(var client in clients) { await client.DeleteAsync("api/Users/me"); client.Dispose(); }
    }
    foreach(var url in uploaded) {
        var name=Path.GetFileName(url);
        var file=Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"SportsBuddyMatcher/SportsBuddyMatcher/wwwroot/uploads",name));
        var folder=Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"SportsBuddyMatcher/SportsBuddyMatcher/wwwroot/uploads"))+Path.DirectorySeparatorChar;
        if(file.StartsWith(folder,StringComparison.OrdinalIgnoreCase)) File.Delete(file);
    }
}
static MultipartFormDataContent Photo(byte[] bytes,string name) { var form=new MultipartFormDataContent(); form.Add(new ByteArrayContent(bytes),"file",name); return form; }
static async Task<T> Post<T>(HttpClient client,string path,object body) { var response=await client.PostAsJsonAsync(path,body); response.EnsureSuccessStatusCode(); return (await response.Content.ReadFromJsonAsync<T>())!; }
static async Task Login(HttpClient client,string email,string password) { var auth=await Post<AuthResponse>(client,"api/Auth/login",new LoginRequest{Email=email,Password=password}); client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",auth.AccessToken); }
static void Check(bool value,string message) {if(!value) throw new Exception("Failed: "+message);}
