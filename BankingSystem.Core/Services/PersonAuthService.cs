using BankingSystem.Core.ServiceContracts;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;


namespace BankingSystem.Core.Services;

public class PersonAuthService(IConfiguration configuration, UserManager<IdentityPerson> userManager, RoleManager<IdentityRole> roleManager) : IPersonAuthService
{
    public async Task<AuthenticationResponse> AuthenticationPersonAsync(PersonLoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return new AuthenticationResponse
            {
                Token = string.Empty,
                Email = loginDto.Email,
                UserId = string.Empty,
                Expiration = DateTime.MinValue,
                ErrorMessage = "Invalid email or password"
            };
        }
        var tokenResponse = await GenerateJwtToken(user);
        return tokenResponse;
    }


    public async Task<bool> RegisterPersonAsync(PersonRegisterDto registerDto)
    {
        var user = new IdentityPerson
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            Name = registerDto.Name,
            Lastname = registerDto.Lastname,
            BirthDate = registerDto.BirthDate,
            IdNumber = registerDto.IdNumber
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
            return false;

        if (string.IsNullOrEmpty(registerDto.Role))
            registerDto.Role = "User";

        if (!await roleManager.RoleExistsAsync(registerDto.Role))
            return false;

        await userManager.AddToRoleAsync(user, registerDto.Role);

        return true;
    }

    public async Task<AuthenticationResponse> GenerateJwtToken(IdentityPerson user)
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
        var expiration = DateTime.UtcNow.AddHours(1);
        
        var tokenGenerator = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: expiration,
            signingCredentials: credentials
        );
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        string token = tokenHandler.WriteToken(tokenGenerator);
        
        return new AuthenticationResponse()
        {
            Token = token, 
            Email = user.Email, 
            UserId = user.Id, 
            Expiration = expiration
        };
    }
}
