using DotNetEnv;
using InternetBank.UI.Configure;
using InternetBank.UI.Middlewares;
using BankingSystem.Infrastructure.Data.DataSeeder;
using InternetBank.UI.ActionFilters;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ModelValidationActionFilter>();
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplicationServices();
builder.Services.AddBankAccountServices();
builder.Services.AddBankCardServices();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddPersonServices();
builder.Services.AddTransactionServices();

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