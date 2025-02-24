namespace BankingSystem.Core.ServiceContracts;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}