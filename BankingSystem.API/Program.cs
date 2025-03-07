using System.Net.Mail;
using BankingSystem.API.ActionFilters;
using BankingSystem.API.Configuration;
using DotNetEnv;
using BankingSystem.Infrastructure.Data.DataSeeder;
using BankingSystem.API.Middlewares;
using BankingSystem.Core.Configuration;
using BankingSystem.Infrastructure.Configure;
using BankingSystem.Infrastructure.Data.DatabaseConfiguration;
using Serilog;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => { options.Filters.Add<ModelValidationActionFilter>(); });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCoreServices();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddMemoryCache();
builder.Host.UseSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();