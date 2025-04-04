﻿using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.ConfigurationSettings.AccountTransaction;
using BankingSystem.Domain.ConfigurationSettings.AtmTransaction;
using BankingSystem.Domain.ConfigurationSettings.Email;
using BankingSystem.Domain.ConfigurationSettings.Jwt;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BankingSystem.Core.Configuration;

public static class ServiceRegistration
{
    public static void AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailConfiguration>(configuration.GetSection("EmailConfiguration"));
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<AccountTransactionSettings>(configuration.GetSection("AccountTransaction"));
        services.Configure<AtmTransactionSettings>(configuration.GetSection("AtmTransaction"));

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/banking_system.log", rollingInterval: RollingInterval.Infinite)
            .CreateLogger();
        services.AddValidatorsFromAssembly(typeof(ServiceRegistration).Assembly,includeInternalTypes:true);
        services.AddFluentValidationAutoValidation();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<IBankCardService, BankCardService>();
        services.AddScoped<IAtmService, AtmService>();
        services.AddScoped<IAccountTransactionService, AccountTransactionService>();
        services.AddScoped<IPersonAuthService, PersonAuthService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IBankReportService, BankReportService>();
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IHasherService, HashingService>(); 
        services.AddScoped<IAuthTokenGeneratorService, AuthTokenGeneratorService>();
        services.Configure<FormOptions>(o => {
            o.ValueLengthLimit = int.MaxValue;
            o.MultipartBodyLengthLimit = int.MaxValue;
            o.MemoryBufferThreshold = int.MaxValue;
        });
        services.AddScoped<IExchangeService, ExchangeService>();
    }
}