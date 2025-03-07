﻿using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BankingSystem.Core.Configuration;

public static class ServiceRegistration
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/banking_system.log", rollingInterval: RollingInterval.Infinite)
            .CreateLogger();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<IBankCardService, BankCardService>();
        services.AddScoped<IAtmService, AtmService>();
        services.AddScoped<IAccountTransactionService, AccountTransactionService>();
        services.AddScoped<IPersonAuthService, PersonAuthService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IBankReportService, BankReportService>();
        services.AddScoped<ILoggerService, LoggerService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IHasherService, HashingService>(); 
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IJwtTokenGeneratorService, JwtTokenGeneratorService>();
        services.Configure<FormOptions>(o => {
            o.ValueLengthLimit = int.MaxValue;
            o.MultipartBodyLengthLimit = int.MaxValue;
            o.MemoryBufferThreshold = int.MaxValue;
        });
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IExchangeService, ExchangeService>();
    }
}