using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace ProyectoKanban.Services
{
    public class EmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var s = _config.GetSection("MailSettings");

            using var client = new SmtpClient(s["Host"], int.Parse(s["Port"]))
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(s["Username"], s["Password"])
            };

            var message = new MailMessage
            {
                From = new MailAddress(s["From"], s["DisplayName"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(to);
            await client.SendMailAsync(message);

            try
{
    await client.SendMailAsync(message);
}
catch (SmtpException ex)
{
    Console.WriteLine($"🚨 SMTP Error: {ex.StatusCode} - {ex.Message}");
    Console.WriteLine("👉 Verifica las credenciales y host de Mailtrap.");
    throw;
}

        }
    }
}
