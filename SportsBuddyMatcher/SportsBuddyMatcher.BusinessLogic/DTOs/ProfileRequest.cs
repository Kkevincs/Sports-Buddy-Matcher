using System.ComponentModel.DataAnnotations;
namespace SportsBuddyMatcher.BusinessLogic.DTOs;
public class ProfileRequest
{
    [Required, MaxLength(255)]
    public required string Name { get; set; }
    [MaxLength(2000)]
    public string? Bio { get; set; }
}
public class ConnectionRequest
{
    [Range(1, int.MaxValue)]
    public required int TargetUserId { get; set; }
}
