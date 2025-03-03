using Microsoft.AspNetCore.Http;
using MimeKit;

namespace BankingSystem.Domain.Entities.Email
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public IFormFileCollection Attachments { get; set; }

        public Message(IEnumerable<string> to, string subject, string content,IFormFileCollection attachments)
        {
            To = to.Select(email => new MailboxAddress("BankingSystem", email)).ToList();
            Subject = subject;
            Content = content;  
            Attachments = attachments;
        }
    }
}