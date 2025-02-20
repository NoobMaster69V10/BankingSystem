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
    public async Task<AdvancedApiResponse<string>> AuthenticationPersonAsync(PersonLoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return AdvancedApiResponse<string>.ErrorResponse("Invalid email or password");
        }

        var token = await GenerateJwtToken(user);
        return AdvancedApiResponse<string>.SuccessResponse(token);
    }

    public async Task<AdvancedApiResponse<string>> RegisterPersonAsync(PersonRegisterDto registerDto)
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
            return AdvancedApiResponse<string>.ErrorResponse(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        if (string.IsNullOrEmpty(registerDto.Role))
            registerDto.Role = "User";

        if (!await roleManager.RoleExistsAsync(registerDto.Role))
        {
            return AdvancedApiResponse<string>.ErrorResponse($"The role '{registerDto.Role}' does not exist.");
        }

        await userManager.AddToRoleAsync(user, registerDto.Role);

        return AdvancedApiResponse<string>.SuccessResponse(default!, "User successfully registered.");
    }

    public async Task<string> GenerateJwtToken(IdentityPerson person)
    {
        var jwtKey = configuration["Jwt:Key"];
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];
        var expirationMinutes = configuration["Jwt:EXPIRATION_MINUTES"];

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience) || string.IsNullOrEmpty(expirationMinutes))
        {
            throw new Exception("JWT configuration is missing.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await userManager.GetRolesAsync(person);
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

        var claims = new List<Claim>
        {
            new Claim("personId", person.Id)
        };

        claims.AddRange(roleClaims);

        var expiration = DateTime.UtcNow.AddMinutes(int.Parse(expirationMinutes));

        var tokenGenerator = new JwtSecurityToken(
            jwtIssuer,
            jwtAudience,
            claims,
            expires: expiration,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.WriteToken(tokenGenerator);

        return token;
    }
}