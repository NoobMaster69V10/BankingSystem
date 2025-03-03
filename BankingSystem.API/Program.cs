using System.Net.Mail;
using BankingSystem.API.ActionFilters;
using BankingSystem.API.Configuration;
using DotNetEnv;
using BankingSystem.Infrastructure.Data.DataSeeder;
using BankingSystem.API.Middlewares;
using BankingSystem.Core.Configuration;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Entities.Email;
using BankingSystem.Infrastructure.Configure;
using BankingSystem.Infrastructure.Data.DatabaseConfiguration;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => { options.Filters.Add<ModelValidationActionFilter>(); });

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddFluentEmail("bankingsystemcredo@gmail.com")
    .AddSmtpSender(new SmtpClient("smtp.gmail.com")
    {
        Port = 587,
        Credentials = new System.Net.NetworkCredential("bankingsystemcredo@gmail.com", "imtm pass skrt tnjq"),
        EnableSsl = true
    });

var emailConfig = builder.Configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCoreServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();



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