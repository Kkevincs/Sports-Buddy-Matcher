using SportsBuddyMatcher.BusinessLogic.Interfaces;
namespace SportsBuddyMatcher.Infrastructure;
public class LocalPhotoStorage : IPhotoStorage
{
    private readonly IWebHostEnvironment _environment;
    public LocalPhotoStorage(IWebHostEnvironment environment) { _environment = environment; }
    public async Task<string> SaveAsync(byte[] content)
    {
        if (content.Length == 0 || content.Length > 5 * 1024 * 1024) throw new ArgumentException("Choose a photo up to 5 MB.");
        var png = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        string extension;
        if (content.AsSpan().StartsWith(png)) extension = ".png";
        else if (content.Length > 3 && content[0] == 255 && content[1] == 216 && content[2] == 255) extension = ".jpg";
        else if (content.Length >= 12 && System.Text.Encoding.ASCII.GetString(content, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(content, 8, 4) == "WEBP") extension = ".webp";
        else throw new ArgumentException("Use a JPEG, PNG, or WebP photo.");
        var folder = Path.Combine(_environment.WebRootPath, "uploads");
        Directory.CreateDirectory(folder);
        var name = Guid.NewGuid().ToString("N") + extension;
        await File.WriteAllBytesAsync(Path.Combine(folder, name), content);
        return "/uploads/" + name;
    }
}
