using System.Text;
using System.Security.Claims;
using BankingSystem.Core.DTO;
using BankingSystem.Core.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using BankingSystem.Core.DTO.Result;
using Microsoft.Extensions.Configuration;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;

namespace BankingSystem.Core.Services;

public class PersonAuthService(
    IConfiguration configuration,
    UserManager<IdentityPerson> userManager,
    RoleManager<IdentityRole> roleManager,
    ILoggerService loggerService) : IPersonAuthService
{
    public async Task<CustomResult<string>> AuthenticationPersonAsync(PersonLoginDto loginDto)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return CustomResult<string>.Failure(CustomError.RecordNotFound("Invalid email or password"));
            }

            var token = await GenerateJwtToken(user);
            return CustomResult<string>.Success(token);
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.Message);
            return CustomResult<string>.Failure(CustomError.ServerError("Error occurred while authenticating person"));
        }
    }

    public async Task<CustomResult<PersonRegisterDto>> RegisterPersonAsync(PersonRegisterDto registerDto)
    {
        try
        {
            var person = new IdentityPerson
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.Name,
                Lastname = registerDto.Lastname,
                BirthDate = registerDto.BirthDate,
                IdNumber = registerDto.IdNumber
            };

            var result = await userManager.CreateAsync(person, registerDto.Password);

            if (!result.Succeeded)
            {
                return CustomResult<PersonRegisterDto>.Failure(new CustomError("UNEXPECTED_ERROR", string.Join(" ", result.Errors.Select(e => e.Description))));
            }

            if (string.IsNullOrEmpty(registerDto.Role))
                registerDto.Role = "User";

            if (!await roleManager.RoleExistsAsync(registerDto.Role))
            {
                return CustomResult<PersonRegisterDto>.Failure(CustomError.RecordNotFound($"The role '{registerDto.Role}' does not exist."));
            }

            await userManager.AddToRoleAsync(person, registerDto.Role);

            return CustomResult<PersonRegisterDto>.Success(registerDto);
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.Message);
            return CustomResult<PersonRegisterDto>.Failure(CustomError.ServerError("Error occurred while authenticating person"));
        }
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