using System.ComponentModel.DataAnnotations;
namespace SportsBuddyMatcher.BusinessLogic.DTOs;
public class LoginRequest
{
    [Required, EmailAddress]
    public required string Email { get; set; }
    [Required]
    public required string Password { get; set; }
}
