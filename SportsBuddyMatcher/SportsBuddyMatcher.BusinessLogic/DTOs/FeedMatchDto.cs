namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public record FeedMatchDto(string Type, int Id, string Name, double Score,
    DateTime? Date = null, DateTime? EndDate = null, string? Address = null,
    int? MaxParticipants = null, int ParticipantCount = 0, string? Bio = null,
    string? PictureUrl = null, string? City = null, double DistanceKm = 0, int? SkillLevel = null);
