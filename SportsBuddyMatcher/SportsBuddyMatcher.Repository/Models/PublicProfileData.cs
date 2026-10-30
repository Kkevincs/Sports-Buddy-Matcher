namespace SportsBuddyMatcher.Repository.Models;

public record PublicSportData(int SkillId, string Name, int SkillLevel);
public record PublicProfileData(int Id, string Name, string? Bio, string? ProfilePictureUrl,
    string City, string Country, List<PublicSportData> Sports);
public record ConnectionData(int Id, int OtherUserId, int RequestedById, bool IsAccepted,
    string Name, string? Bio, string? ProfilePictureUrl, string City);
