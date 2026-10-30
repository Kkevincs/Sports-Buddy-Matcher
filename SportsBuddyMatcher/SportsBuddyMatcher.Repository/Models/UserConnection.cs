namespace SportsBuddyMatcher.Repository.Models;

public class UserConnection
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int OtherUserId { get; set; }
    public int RequestedById { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public User OtherUser { get; set; } = null!;
}
