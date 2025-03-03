using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities.Email;
using MailKit.Net.Smtp;
using MimeKit;

namespace BankingSystem.Core.Services;

public class EmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfig;
    private readonly ILoggerService _loggerService;

    public EmailService(EmailConfiguration emailConfig)
    {
        _emailConfig = emailConfig;
    }

    public async Task SendEmailAsync(Message message)
    {
        var emailMessage = CreateEmailMessage(message);
        await SendAsync(emailMessage);
    }

    private MimeMessage CreateEmailMessage(Message message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("BankingSystem", _emailConfig.From));
        emailMessage.To.AddRange(message.To);
        emailMessage.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = GetHtmlBody(message)
        };

        if (message.Attachments.Any())
        {
            byte[] fileBytes;
            foreach (var attachment in message.Attachments)
            {
                using (var ms = new MemoryStream())
                {
                    attachment.CopyTo(ms);
                    fileBytes = ms.ToArray();
                }

                var contentType = attachment.ContentType.ToString().Split('/');
                var mediaType = contentType[0];
                var subType = contentType[1];

                bodyBuilder.Attachments.Add(attachment.FileName, fileBytes, new ContentType(mediaType, subType));
            }
        }

        emailMessage.Body = bodyBuilder.ToMessageBody();
        return emailMessage;
    }

    private async Task SendAsync(MimeMessage mailMessage)
    {
        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);

            await client.SendAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _loggerService.LogErrorInConsole(ex.Message);
        }
        finally
        {
            await client.DisconnectAsync(true);
            client.Dispose();
        }
    }

    private string GetHtmlBody(Message message)
    {
        return $@"
        <html>
            <body style='font-family: Arial, sans-serif; color: #333; margin: 0; padding: 0; background-color: #f4f4f4;'>
                <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                    <tr>
                        <td>
                            <!-- Main Email Container -->
                            <table width='600' cellpadding='0' cellspacing='0' style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);'>
                                <tr>
                                    <td style='text-align: center; padding-bottom: 20px;'>
                                        <!-- Logo or Company Name -->
                                        <h1 style='color: #2c3e50; font-size: 24px; margin: 0;'>Banking System</h1>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <!-- Body Content -->
                                        <p style='font-size: 16px; color: #555; line-height: 1.5;'>Hello,</p>
                                        <p style='font-size: 16px; color: #555; line-height: 1.5;'>We received a request to reset your password for your Banking System account. If you didn't make this request, you can safely ignore this email.</p>
                                        <p style='font-size: 16px; color: #555; line-height: 1.5;'>To reset your password, click the button below:</p>
                                        <div style='text-align: center; margin: 30px 0;'>
                                            <a href='{message.Content}' style='background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-size: 18px; font-weight: bold; display: inline-block;'>Reset Password</a>
                                        </div>
                                        <p style='font-size: 16px; color: #555; line-height: 1.5;'>If the button doesn't work, you can copy and paste this link into your browser:</p>
                                        <p style='font-size: 14px; color: #666; word-break: break-all; line-height: 1.5;'>{message.Content}</p>
                                        <p style='font-size: 14px; color: #999; line-height: 1.5;'>This link will expire in 24 hours.</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='text-align: center; padding-top: 30px; font-size: 14px; color: #888;'>
                                        <p style='margin: 0;'>Best regards,</p>
                                        <p style='margin: 0;'>The Banking System Team</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
        </html>";
    }
}