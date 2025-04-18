﻿using Microsoft.AspNetCore.Identity;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using BankingSystem.Core.DTO.Person;
using BankingSystem.Core.DTO.RefreshToken;
using BankingSystem.Core.Identity;
using BankingSystem.Domain.ConfigurationSettings.Email;
using Microsoft.AspNetCore.WebUtilities;
using BankingSystem.Core.Response;
using BankingSystem.Core.Result;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BankingSystem.Core.Services;

public class PersonAuthService : IPersonAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityPerson> _userManager;
    private readonly IEmailService _emailService;
    private readonly IAuthTokenGeneratorService _tokenGenerator;
    private readonly ILoggerService _loggerService;
    private readonly IHttpContextAccessor _contextAccessor;

    public PersonAuthService(IUnitOfWork unitOfWork, UserManager<IdentityPerson> userManager, IEmailService emailService, IAuthTokenGeneratorService tokenGenerator, IHttpContextAccessor contextAccessor, ILoggerService loggerService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailService = emailService;
        _tokenGenerator = tokenGenerator;
        _contextAccessor = contextAccessor;
        _loggerService = loggerService;
    }


    public async Task<Result<AuthenticatedResponse>> AuthenticationPersonAsync(PersonLoginDto loginDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Result<AuthenticatedResponse>.Failure(CustomError.NotFound("Invalid email"));
            }

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                await _userManager.AccessFailedAsync(user);
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var content = $"Your account has been locked. Please check your email and password. you can use the forgot password link";
                    var message = new Message([loginDto.Email], "Locked out account information", content, null!);
                    await _emailService.SendEmailAsync(message, "Reset Password", cancellationToken);

                    return Result<AuthenticatedResponse>.Failure(CustomError.AccessUnAuthorized("The account has been locked."));
                }

                return Result<AuthenticatedResponse>.Failure(CustomError.NotFound("Invalid password"));
            }
            var token = await _tokenGenerator.GenerateAccessTokenAsync(user);
            var refreshToken = new RefreshToken
            {
                Token = _tokenGenerator.GenerateRefreshToken(),
                ExpiresOnUtc = DateTime.UtcNow.AddDays(7),
                PersonId = user.Id
            };
            var existedPerson = await _unitOfWork.RefreshTokenRepository.CheckPersonIdAsync(user.Id, cancellationToken);
            if (existedPerson == null)
            {
                await _unitOfWork.RefreshTokenRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
            }
            else
            {
                await _unitOfWork.RefreshTokenRepository.UpdateRefreshTokenAsync(refreshToken, cancellationToken);
            } 
            var response = new AuthenticatedResponse
            {
                Token = token,
                RefreshToken = refreshToken.Token
            };
            await _userManager.ResetAccessFailedCountAsync(user);
            return Result<AuthenticatedResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex.Message);
            return Result<AuthenticatedResponse>.Failure(CustomError.Failure("Error occurred while authenticating person"));
        }
    }


    public async Task<Result<RegisterResponse>> RegisterPersonAsync(PersonRegisterDto registerDto, CancellationToken cancellationToken = default)
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

            var testIdNumber = await _userManager.Users.FirstOrDefaultAsync(u => u.IdNumber == registerDto.IdNumber, cancellationToken);

            if (testIdNumber != null)
            {
                return Result<RegisterResponse>.Failure(CustomError.Validation($"The idNumber '{registerDto.IdNumber}' already exits."));
            }

            var result = await _userManager.CreateAsync(person, registerDto.Password);

            if (!result.Succeeded)
            {
                return Result<RegisterResponse>.Failure(new CustomError("UNEXPECTED_ERROR",
                    string.Join(" ", result.Errors.Select(e => e.Description))));
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(person);
            
            var param = new Dictionary<string, string?>
            {
                { "token", token },
                { "email", person.Email }
            };

            var currentUrl = _contextAccessor.HttpContext?.Request.Host.Value;
            var callback = QueryHelpers.AddQueryString($"https://{currentUrl}/api/Person/email-confirmation", param);
            var message = new Message([person.Email], "Email Confirmation", callback, null!);

            _ = Task.Run(async () =>
            {
                await _emailService.SendEmailAsync(message, "Confirm Account", cancellationToken);
            }, cancellationToken);
            await _userManager.AddToRoleAsync(person, registerDto.Role.ToString());
            
            var response = new RegisterResponse
            {
                FirstName = person.FirstName,
                Lastname = person.Lastname,
                IdNumber = person.IdNumber,
                BirthDate = person.BirthDate,
                Email = person.Email,
                Role = registerDto.Role,
            };

            return Result<RegisterResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex.Message);
            return Result<RegisterResponse>.Failure(
                CustomError.Failure("Error occurred while authenticating person"));
        }
    }

    public async Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
        {
            return Result<string>.Failure(CustomError.NotFound("Invalid email"));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var param = new Dictionary<string, string>
        {
            { "token", token },
            { "email", forgotPasswordDto.Email }
        };
        var callback = QueryHelpers.AddQueryString("",param!);
        var message = new Message([user.Email!], "EmailConfirmation", callback, null!);
        await _emailService.SendEmailAsync(message, "Reset Password", cancellationToken);
        return Result<string>.Success("Email sent successfully.");
    }

    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken = default)
    {
        var decodedToken = System.Web.HttpUtility.UrlDecode(resetPasswordDto.Token);
        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null)
        {
            return Result<bool>.Failure(CustomError.NotFound("User not found"));
        }

        var resetPasswordResult =
            await _userManager.ResetPasswordAsync(user, decodedToken, resetPasswordDto.NewPassword);
        if (!resetPasswordResult.Succeeded)
        {
            return Result<bool>.Failure(CustomError.NotFound("Invalid or expired token"));
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        return Result<bool>.Success(true);
    }

    public async Task<Result<string>> EmailConfirmationAsync(string token, string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result<string>.Failure(CustomError.NotFound("User Not Found"));
        }

        var confirmResult = await _userManager.ConfirmEmailAsync(user, token);
        if (!confirmResult.Succeeded)
        {
            var errors = string.Join(", ", confirmResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
            _loggerService.LogError($"Email confirmation failed: {errors}");
            return Result<string>.Failure(CustomError.Validation("Invalid email confirmation request"));
        }

        return Result<string>.Success("Email Confirmed successfully.");
    }
    public async Task<Result<AuthenticatedResponse>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var principal = _tokenGenerator.GetPrincipalFromExpiredToken(refreshTokenDto.Token);
            var personId = principal.FindFirst("personId")?.Value;
            
            if (string.IsNullOrEmpty(personId))
            {
                return Result<AuthenticatedResponse>.Failure(
                    CustomError.AccessUnAuthorized("Invalid Request"));
            }

            var storedRefreshToken = await _unitOfWork.RefreshTokenRepository
                .GetDataByToken(refreshTokenDto.RefreshToken, cancellationToken);
            
            if (storedRefreshToken == null || 
                storedRefreshToken.PersonId != personId || 
                storedRefreshToken.ExpiresOnUtc <= DateTime.UtcNow)
            {
                _loggerService.LogError("Invalid refresh token");
                return Result<AuthenticatedResponse>.Failure(
                    CustomError.AccessUnAuthorized("Invalid Request"));
            }
            var user = await _userManager.FindByIdAsync(personId);
            if (user == null)
            {
                return Result<AuthenticatedResponse>.Failure(
                    CustomError.NotFound("User not found"));
            }

            var newAccessToken = await _tokenGenerator.GenerateAccessTokenAsync(user);

            var newRefreshToken = new RefreshToken
            {
                Token = _tokenGenerator.GenerateRefreshToken(),
                ExpiresOnUtc = DateTime.UtcNow.AddDays(7),
                PersonId = user.Id
            };

            storedRefreshToken.Token = newRefreshToken.Token;
            storedRefreshToken.ExpiresOnUtc = newRefreshToken.ExpiresOnUtc;

            await _unitOfWork.RefreshTokenRepository.UpdateRefreshTokenAsync(storedRefreshToken, cancellationToken);
            var response = new AuthenticatedResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
            
            return Result<AuthenticatedResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex.Message);
            return Result<AuthenticatedResponse>.Failure(
                CustomError.Failure("Error occurred while refreshing token"));
        }
    }
}