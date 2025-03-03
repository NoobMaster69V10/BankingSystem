using BankingSystem.Domain.Entities.Email;

namespace BankingSystem.Core.ServiceContracts;

public interface IEmailService
{
    Task SendEmailAsync(Message message);
}