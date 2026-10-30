namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public class EventDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime Date { get; set; }
    public DateTime? EndDate { get; set; }
    public int? SkillId { get; set; }
    public int? SkillLevel { get; set; }
    public int Location { get; set; }
    public int? MaxParticipants { get; set; }
    public int ParticipantCount { get; set; }
    public string? Bio { get; set; }
    public string? PictureUrl { get; set; }
    public int? CreatedById { get; set; }
}
