using System.Net.Mail;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using DotNetEnv;
using InternetBank.UI.Middlewares;
using BankingSystem.Infrastructure.Data.DataSeeder;
using BankingSytem.API.ActionFilters;
using BankingSytem.API.Configure;

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

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddApplicationServices();
builder.Services.AddBankAccountServices();
builder.Services.AddBankCardServices();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddPersonServices();
builder.Services.AddTransactionServices();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<ApplicationDataSeeder>();
    await seeder.Seed();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();