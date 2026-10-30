using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SportsBuddyMatcher.BusinessLogic.DTOs;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.Repository.Models;
namespace SportsBuddyMatcher.Security;
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    public TokenService(IConfiguration configuration) { _configuration = configuration; }
    public AuthResponse CreateToken(User user)
    {
        var expires = DateTime.UtcNow.AddHours(1);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: new[] { new Claim("sub", user.Id.ToString()), new Claim("jti", Guid.NewGuid().ToString()) },
            expires: expires,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_SECRET"]!)), SecurityAlgorithms.HmacSha256));
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
