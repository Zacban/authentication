using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityNetCore.Abstractions;
using IdentityNetCore.Options;
using Microsoft.Extensions.Options;

namespace IdentityNetCore.Services
{
    public class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailService
    {
        private readonly SmtpOptions _smtpOptions = options.Value;

        public async Task SendEmailAsync(string from, string to, string subject, string body)
        {
            var mailMessage = new System.Net.Mail.MailMessage(from, to, subject, body);
            using var smtpClient = new System.Net.Mail.SmtpClient(_smtpOptions.Host, _smtpOptions.Port);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}