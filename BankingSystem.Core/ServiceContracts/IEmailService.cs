using BankingSystem.Domain.ConfigurationSettings.Email;

namespace BankingSystem.Core.ServiceContracts;

public interface IEmailService
{
    Task SendEmailAsync(Message message, string buttonText, CancellationToken cancellationToken = default);
}