namespace SportsBuddyMatcher.BusinessLogic.DTOs;

public class AvailabilityDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
