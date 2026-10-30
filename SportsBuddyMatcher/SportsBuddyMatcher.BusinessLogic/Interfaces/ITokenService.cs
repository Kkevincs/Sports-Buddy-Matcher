using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.Repository.Models;
namespace SportsBuddyMatcher.BusinessLogic.Interfaces;
public interface ITokenService { AuthResponse CreateToken(User user); }
