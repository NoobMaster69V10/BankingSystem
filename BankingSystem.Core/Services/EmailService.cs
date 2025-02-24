using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using FluentEmail.Core;

namespace BankingSystem.Core.Services;


public class EmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;

    public EmailService(IFluentEmail fluentEmail)
    {
        _fluentEmail = fluentEmail;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        await _fluentEmail
            .To(to)
            .Subject(subject)
            .Body(body, isHtml: true)
            .SendAsync();
    }

}