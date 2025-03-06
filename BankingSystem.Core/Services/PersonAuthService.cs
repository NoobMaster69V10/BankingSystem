using BankingSystem.Core.Identity;
using Microsoft.AspNetCore.Identity;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using BankingSystem.Core.DTO.Person;
using BankingSystem.Domain.ConfigurationSettings.Email;
using Microsoft.AspNetCore.WebUtilities;
using BankingSystem.Core.DTO.Response;

namespace BankingSystem.Core.Services;

public class PersonAuthService(
    UserManager<IdentityPerson> userManager,
    RoleManager<IdentityRole> roleManager,
    ILoggerService loggerService,
    IEmailService emailService,IJwtTokenGenerator tokenGenerator) : IPersonAuthService
{
    public async Task<Result<AuthenticatedResponse>> AuthenticationPersonAsync(PersonLoginDto loginDto)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Result<AuthenticatedResponse>.Failure(CustomError.NotFound("Invalid email"));

            }
            if (!await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Result<AuthenticatedResponse>.Failure(CustomError.NotFound("Invalid password"));
            }

            var token = await tokenGenerator.GenerateJwtToken(user);
            var response = new AuthenticatedResponse
            {
                Token = token
            };
            return Result<AuthenticatedResponse>.Success(response);
        }
        catch (Exception ex)
        {
            loggerService.LogError(ex.Message);
            return Result<AuthenticatedResponse>.Failure(CustomError.Failure("Error occurred while authenticating person"));
        }
    }


    public async Task<Result<RegisterResponse>> RegisterPersonAsync(PersonRegisterDto registerDto)
    {
        try
        {
            var person = new IdentityPerson
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                Lastname = registerDto.Lastname,
                BirthDate = registerDto.BirthDate,
                IdNumber = registerDto.IdNumber
            };

            var result = await userManager.CreateAsync(person, registerDto.Password);

            if (!result.Succeeded)
            {
                return Result<RegisterResponse>.Failure(new CustomError("UNEXPECTED_ERROR",
                    string.Join(" ", result.Errors.Select(e => e.Description))));
            }
            var token = await userManager.GenerateEmailConfirmationTokenAsync(person);
            var param = new Dictionary<string, string?>
            {
                { "token", token },
                { "email", person.Email }
            };
            var callback = QueryHelpers.AddQueryString(registerDto.ClientUri!, param);
            var message = new Message([person.Email], "Email Confirmation Token", callback,null);
            await emailService.SendEmailAsync(message);
            
            var role = string.IsNullOrEmpty(registerDto.Role) ? "Person" : registerDto.Role;

            if (!await roleManager.RoleExistsAsync(role))
            {
                return Result<RegisterResponse>.Failure(
                    CustomError.NotFound($"The role '{role}' does not exist."));
            }

            await userManager.AddToRoleAsync(person, role);

            var response = new RegisterResponse
            {
                FirstName = person.FirstName,
                Lastname = person.Lastname,
                IdNumber = person.IdNumber,
                BirthDate = person.BirthDate,
                Email = person.Email,
                Role = role
            };

            return Result<RegisterResponse>.Success(response);
        }
        catch (Exception ex)
        {
            loggerService.LogError(ex.Message);
            return Result<RegisterResponse>.Failure(
                CustomError.Failure("Error occurred while authenticating person"));
        }
    }

    public async Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
        {
            return Result<string>.Failure(CustomError.NotFound("Invalid email"));
        }
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var param = new Dictionary<string, string>
        {
            { "token", token },
            { "email", forgotPasswordDto.Email }
        };
        var callback = QueryHelpers.AddQueryString(forgotPasswordDto.ClientUri, param!);
        var message = new Message([user.Email],"EmailConfirmation",callback,null!);
        await emailService.SendEmailAsync(message);
        return Result<string>.Success("Email sent successfully.");
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

    public async  Task<Result<string>> EmailConfirmationAsync(EmailConfirmationDto emailConfirmationDto)
    {
        var user = await userManager.FindByEmailAsync(emailConfirmationDto.Email);
        if (user is null)
        {
            return Result<string>.Failure(CustomError.NotFound("Invalid email confirmation request"));
        }
        var confirmResult = await userManager.ConfirmEmailAsync(user, emailConfirmationDto.Token);
        if (!confirmResult.Succeeded)
        {
            return Result<string>.Failure(CustomError.Validation("Invalid email confirmation request"));
        }
        return Result<string>.Success("Email sent successfully.");
    }
}