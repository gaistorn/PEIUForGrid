using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PEIU.Service.WebApiService
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string senderName, string subject, string message, params string[] emails);
    }

    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly IHostingEnvironment _env;

        public EmailSender(
            IOptions<EmailSettings> emailSettings,
            IHostingEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _env = env;
        }

        public async Task SendEmailAsync(string senderName, string subject, string message, params string[] emails)
        {
            try
            {
                //await Task.CompletedTask;
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(System.Text.Encoding.UTF8, senderName, _emailSettings.Sender));
                IEnumerable< MailboxAddress> address = emails.Select(x => new MailboxAddress(x));
                mimeMessage.To.AddRange(address);

                mimeMessage.Subject = subject;
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = message;
                mimeMessage.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync(_emailSettings.MailServer, _emailSettings.MailPort, true);

                    // Note: only needed if the SMTP server requires authentication
                    await client.AuthenticateAsync(_emailSettings.Sender, _emailSettings.Password);

                    await client.SendAsync(mimeMessage);

                    await client.DisconnectAsync(true);
                }

            }
            catch (Exception ex)
            {
                // TODO: handle exception
                throw new InvalidOperationException(ex.Message);
            }
        }
    }

    public class EmailSettings
    {
        public string MailServer { get; set; }
        public int MailPort { get; set; }
        public string SenderName { get; set; }
        public string Sender { get; set; }
        public string Password { get; set; }
    }
}
