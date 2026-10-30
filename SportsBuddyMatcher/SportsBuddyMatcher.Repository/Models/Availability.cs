namespace SportsBuddyMatcher.Repository.Models;

public class Availability
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public User User { get; set; } = null!;
}
