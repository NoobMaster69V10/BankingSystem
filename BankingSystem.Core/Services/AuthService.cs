using BankingSystem.Core.ServiceContracts;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using BankingSystem.Core.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;


namespace BankingSystem.Core.Services;

public class AuthService(IConfiguration configuration, UserManager<IdentityPerson> userManager) : IAuthService
{
    public async Task<string> GenerateJwtToken(IdentityPerson user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", user.Id)
        };

        claims.AddRange(roleClaims);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
