using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace AgroConnect.Infrastructure.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Bura öz real Gmail ünvanınızı yazın
            var username = "ccemo4239@gmail.com";

            // Bura Google-dan aldığınız 16 hərfli "App Password"u yazın (Məsələn: "abcd efgh ijkl mnop")
            var password = "dant odah nhfd uwfu";

            var host = "smtp.gmail.com";
            var port = 587;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username, "AgroConnect"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }
}