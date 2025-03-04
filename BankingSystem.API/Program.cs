using DotNetEnv;
using BankingSystem.API.Middlewares;
using BankingSystem.API.Configuration;
using BankingSystem.API.ActionFilters;
using BankingSystem.Core.Configuration;
using BankingSystem.Infrastructure.Configure;
using BankingSystem.Infrastructure.Data.DataSeeder;
using BankingSystem.Infrastructure.Data.DatabaseConfiguration;
using Serilog;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => { options.Filters.Add<ModelValidationActionFilter>(); });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCoreServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Host.UseSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseConfiguration>();
    await db.ConfigureDatabase();
}

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<ApplicationDataSeeder>();
    await seeder.Seed();
}

app.UseExceptionHandler();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();