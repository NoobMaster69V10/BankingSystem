using BankingSystem.Core.DTO;
using BankingSystem.Core.Identity;
using Microsoft.AspNetCore.Identity;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using BankingSystem.Core.DTO.Person;
using BankingSystem.Domain.ConfigurationSettings.Email;
using Microsoft.AspNetCore.WebUtilities;
using BankingSystem.Core.Response;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;
using Sprache;

namespace BankingSystem.Core.Services;

public class PersonAuthService(
    UserManager<IdentityPerson> userManager,
    RoleManager<IdentityRole> roleManager,
    ILoggerService loggerService,
    IEmailService emailService,
    IJwtTokenGeneratorService tokenGenerator,
    IUnitOfWork unitOfWork) : IPersonAuthService
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
                await userManager.AccessFailedAsync(user);
                if (await userManager.IsLockedOutAsync(user))
                {
                    var content = $"Your account has been locked. Please check your email and password." +
                                  "you can use the forgot password link";
                    var message = new Message([loginDto.Email], "Locked out account information", content, null!);
                    await emailService.SendEmailAsync(message);
                    return Result<AuthenticatedResponse>.Failure(
                        CustomError.AccessUnAuthorized("The account has been locked."));
                }

                return Result<AuthenticatedResponse>.Failure(CustomError.NotFound("Invalid password"));
            }
            var token = await tokenGenerator.GenerateAccessTokenAsync(user);
            var refreshToken = new RefreshToken
            {
                Token = tokenGenerator.GenerateRefreshToken(),
                ExpiresOnUtc = DateTime.UtcNow.AddDays(7),
                PersonId = user.Id
            };
            var existedPerson = await unitOfWork.RefreshTokenRepository.CheckPersonIdAsync(user.Id);
            if (existedPerson == null)
            {
                await unitOfWork.RefreshTokenRepository.AddRefreshTokenAsync(refreshToken);
            }
            else
            {
                await unitOfWork.RefreshTokenRepository.UpdateRefreshTokenAsync(refreshToken);
            } 
            var response = new AuthenticatedResponse
            {
                Token = token,
                RefreshToken = refreshToken.Token
            };
            await userManager.ResetAccessFailedCountAsync(user);
            return Result<AuthenticatedResponse>.Success(response);
        }
        catch (Exception ex)
        {
            loggerService.LogError(ex.Message);
            return Result<AuthenticatedResponse>.Failure(
                CustomError.Failure("Error occurred while authenticating person"));
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
            var message = new Message([person.Email], "Email Confirmation", callback, null!);
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
        if (!user.EmailConfirmed)
        {
            return Result<string>.Failure(CustomError.Validation("Email not confirmed"));
        }
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var param = new Dictionary<string, string>
        {
            { "token", token },
            { "email", forgotPasswordDto.Email }
        };
        var callback = QueryHelpers.AddQueryString(forgotPasswordDto.ClientUri, param!);
        var message = new Message([user.Email], "EmailConfirmation", callback, null!);
        await emailService.SendEmailAsync(message);
        return Result<string>.Success("Email sent successfully.");
    }

    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var decodedToken = System.Web.HttpUtility.UrlDecode(resetPasswordDto.Token);
        var user = await userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null)
        {
            return Result<bool>.Failure(CustomError.NotFound("User not found"));
        }

        var resetPasswordResult =
            await userManager.ResetPasswordAsync(user, decodedToken, resetPasswordDto.NewPassword);
        if (!resetPasswordResult.Succeeded)
        {
            return Result<bool>.Failure(CustomError.NotFound("Invalid or expired token"));
        }

        await userManager.SetLockoutEndDateAsync(user, null);
        return Result<bool>.Success(true);
    }

    public async Task<Result<string>> EmailConfirmationAsync(string token, string email)
    {
        var decodedToken = System.Web.HttpUtility.UrlDecode(token);

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result<string>.Failure(CustomError.NotFound("User Not Found"));
        }

        var confirmResult = await userManager.ConfirmEmailAsync(user, decodedToken);
        if (!confirmResult.Succeeded)
        {
            var errors = string.Join(", ", confirmResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
            loggerService.LogError($"Email confirmation failed: {errors}");
            return Result<string>.Failure(CustomError.Validation("Invalid email confirmation request"));
        }

        return Result<string>.Success("Email Confirmed successfully.");
    }
    public async Task<Result<AuthenticatedResponse>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
{
    try
    {
        var principal = tokenGenerator.GetPrincipalFromExpiredToken(refreshTokenDto.Token);
        var personId = principal.FindFirst("personId")?.Value;
        
        if (string.IsNullOrEmpty(personId))
        {
            return Result<AuthenticatedResponse>.Failure(
                CustomError.AccessUnAuthorized("Invalid Request"));
        }

        var storedRefreshToken = await unitOfWork.RefreshTokenRepository
            .GetDataByToken(refreshTokenDto.RefreshToken);
        
        if (storedRefreshToken == null || 
            storedRefreshToken.PersonId != personId || 
            storedRefreshToken.ExpiresOnUtc <= DateTime.UtcNow)
        {
            loggerService.LogError("Invalid refresh token");
            return Result<AuthenticatedResponse>.Failure(
                CustomError.AccessUnAuthorized("Invalid Request"));
        }
        var user = await userManager.FindByIdAsync(personId);
        if (user == null)
        {
            return Result<AuthenticatedResponse>.Failure(
                CustomError.NotFound("User not found"));
        }

        var newAccessToken = await tokenGenerator.GenerateAccessTokenAsync(user);
        var newRefreshToken = new RefreshToken
        {
            Token = tokenGenerator.GenerateRefreshToken(),
            ExpiresOnUtc = DateTime.UtcNow.AddDays(7),
            PersonId = user.Id
        };
        storedRefreshToken.Token = newRefreshToken.Token;
        storedRefreshToken.ExpiresOnUtc = newRefreshToken.ExpiresOnUtc;
        await unitOfWork.RefreshTokenRepository.UpdateRefreshTokenAsync(storedRefreshToken);
        var response = new AuthenticatedResponse
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken.Token
        };
        
        return Result<AuthenticatedResponse>.Success(response);
    }
    catch (Exception ex)
    {
        loggerService.LogError(ex.Message);
        return Result<AuthenticatedResponse>.Failure(
            CustomError.Failure("Error occurred while refreshing token"));
    }
}
}