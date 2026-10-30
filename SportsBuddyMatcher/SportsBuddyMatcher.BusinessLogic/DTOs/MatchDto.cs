namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public record MatchDto(int UserId, string Name, double Score, double DistanceKm = 0);
