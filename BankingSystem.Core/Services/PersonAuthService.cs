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
using BankingSystem.Domain.Entities.Email;
using Microsoft.AspNetCore.WebUtilities;

namespace BankingSystem.Core.Services;

public class PersonAuthService(
    IConfiguration configuration,
    UserManager<IdentityPerson> userManager,
    RoleManager<IdentityRole> roleManager,
    ILoggerService loggerService,
    IEmailService emailService,IJwtTokenGenerator tokenGenerator) : IPersonAuthService
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
            if (!await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Result<string>.Failure(CustomError.NotFound("Invalid password"));
            }

            var token = await tokenGenerator.GenerateJwtToken(user);
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
        var callback = QueryHelpers.AddQueryString(forgotPasswordDto.ClientUri, param);
        var message = new Message([user.Email],"Reset Your password",callback,null);
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
}