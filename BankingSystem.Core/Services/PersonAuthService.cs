using System.Text;
using System.Security.Claims;
using BankingSystem.Core.DTO;
using BankingSystem.Core.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using BankingSystem.Core.DTO.Response;
using Microsoft.Extensions.Configuration;
using BankingSystem.Core.ServiceContracts;

namespace BankingSystem.Core.Services;

public class PersonAuthService(
    IConfiguration configuration,
    UserManager<IdentityPerson> userManager,
    RoleManager<IdentityRole> roleManager) : IPersonAuthService
{
    public async Task<AuthenticationResponse> AuthenticationPersonAsync(PersonLoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return new AuthenticationResponse
            {
                IsSuccess = false,
                Token = string.Empty,
                ErrorMessage = "Invalid email or password"
            };
        }

        var tokenResponse = await GenerateJwtToken(user);
        return tokenResponse;
    }

    public async Task<AuthenticationResponse> RegisterPersonAsync(PersonRegisterDto registerDto)
    {
        var user = new IdentityPerson
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.Name,
            Lastname = registerDto.Lastname,
            BirthDate = registerDto.BirthDate,
            IdNumber = registerDto.IdNumber
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return new AuthenticationResponse
            {
                IsSuccess = false,
                ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        if (string.IsNullOrEmpty(registerDto.Role))
            registerDto.Role = "User";

        if (!await roleManager.RoleExistsAsync(registerDto.Role))
        {
            return new AuthenticationResponse
            {
                IsSuccess = false,
                ErrorMessage = $"The role '{registerDto.Role}' does not exist."
            };
        }

        await userManager.AddToRoleAsync(user, registerDto.Role);

        var tokenResponse = await GenerateJwtToken(user);

        return new AuthenticationResponse
        {
            IsSuccess = true,
            Token = tokenResponse.Token
        };
    }

    public async Task<AuthenticationResponse> GenerateJwtToken(IdentityPerson person)
    {
        var jwtKey = configuration["Jwt:Key"];
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
        {
            return new AuthenticationResponse { IsSuccess = false, ErrorMessage = "JWT configuration is missing." };
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await userManager.GetRolesAsync(person);
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, person.Id)
        };

        claims.AddRange(roleClaims);

        var expiration = DateTime.UtcNow.AddHours(1);

        var tokenGenerator = new JwtSecurityToken(
            jwtIssuer,
            jwtAudience,
            claims,
            expires: expiration,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.WriteToken(tokenGenerator);

        return new AuthenticationResponse
        {
            IsSuccess = true,
            Token = token
        };
    }
}