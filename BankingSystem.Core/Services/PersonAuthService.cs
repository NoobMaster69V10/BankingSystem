using System.Text;
using System.Security.Claims;
using BankingSystem.Core.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using BankingSystem.Core.DTO.Result;
using Microsoft.Extensions.Configuration;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using Microsoft.AspNetCore.Http;

using ResetPasswordDto = BankingSystem.Core.DTO.Person.ResetPasswordDto;
using BankingSystem.Core.DTO.Person;

namespace BankingSystem.Core.Services;

public class PersonAuthService(
    IConfiguration configuration,
    UserManager<IdentityPerson> userManager,
    RoleManager<IdentityRole> roleManager,
    ILoggerService loggerService,
    IHttpContextAccessor httpContextAccessor,
    IEmailService emailService) : IPersonAuthService
{
    public async Task<Result<string>> AuthenticationPersonAsync(PersonLoginDto loginDto)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Result<string>.Failure(CustomError.NotFound("Invalid email"));

            }
            if (await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Result<string>.Failure(CustomError.NotFound("Invalid password"));
            }

            var token = await GenerateJwtToken(user);
            return Result<string>.Success(token);
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.Message);
            return Result<string>.Failure(CustomError.Failure("Error occurred while authenticating person"));
        }
    }


    public async Task<Result<PersonRegisterDto>> RegisterPersonAsync(PersonRegisterDto registerDto)
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
                return Result<PersonRegisterDto>.Failure(new CustomError("UNEXPECTED_ERROR",
                    string.Join(" ", result.Errors.Select(e => e.Description))));
            }

            if (string.IsNullOrEmpty(registerDto.Role))
                registerDto.Role = "User";

            if (!await roleManager.RoleExistsAsync(registerDto.Role))
            {
                return Result<PersonRegisterDto>.Failure(
                    CustomError.NotFound($"The role '{registerDto.Role}' does not exist."));
            }

            await userManager.AddToRoleAsync(person, registerDto.Role);

            return Result<PersonRegisterDto>.Success(registerDto);
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.Message);
            return Result<PersonRegisterDto>.Failure(
                CustomError.Failure("Error occurred while authenticating person"));
        }
    }

    public async Task<string> GenerateJwtToken(IdentityPerson person)
    {
        var jwtKey = configuration["Jwt:Key"];
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];
        var expirationMinutes = configuration["Jwt:EXPIRATION_MINUTES"];

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience) ||
            string.IsNullOrEmpty(expirationMinutes))
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

    public async Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
        {
            return Result<string>.Failure(CustomError.NotFound("Invalid email"));
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var request = httpContextAccessor.HttpContext?.Request;
        var resetLink =
            $"{request?.Scheme}://{request.Host}/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        string email = user.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<string>.Failure(CustomError.Validation("User has no valid email address"));
        }

        string emailBody = $@"
<div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 5px;"">
    <h2 style=""color: #333;"">Reset Your Password</h2>
    <p>Hello,</p>
    <p>We received a request to reset your password for your Banking System account. If you didn't make this request, you can safely ignore this email.</p>
    <p>To reset your password, click the button below:</p>
    <div style=""text-align: center; margin: 30px 0;"">
        <a href=""{resetLink}"" style=""background-color: #4CAF50; color: white; padding: 12px 20px; text-decoration: none; border-radius: 4px; font-weight: bold;"">Reset Password</a>
    </div>
    <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
    <p style=""word-break: break-all; color: #666;"">{resetLink}</p>
    <p>This link will expire in 24 hours.</p>
    <p>Regards,<br>Banking System Team</p>
</div>";

        await emailService.SendEmailAsync(email, "Reset Your Password", emailBody);
        return Result<string>.Success(token);
    }
    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var user = await userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null)
        {
            return Result<bool>.Failure(CustomError.NotFound("User not found"));
        }
        var resetPasswordResult = await userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
        if (!resetPasswordResult.Succeeded)
        {
            return Result<bool>.Failure(CustomError.NotFound("Invalid or expired token"));
        }
        return Result<bool>.Success(true);
    }
}