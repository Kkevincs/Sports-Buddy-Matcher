namespace SportsBuddyMatcher.BusinessLogic.DTOs;
public record PublicSportDto(int SkillId, string Name, int SkillLevel);
public record PublicProfileDto(int Id, string Name, string? Bio, string? ProfilePictureUrl,
    string City, string Country, List<PublicSportDto> Sports);
public record ConnectionDto(int Id, int OtherUserId, string Name, string? Bio,
    string? ProfilePictureUrl, string City, string Status, bool IsIncoming);
