using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PegasusV1.Interfaces;

namespace PegasusV1.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task EnviarEmailAsync(string destinatario, string asunto, string contenido)
        {
            try
            {
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
                var smtpUsername = _configuration["Email:Username"];
                var smtpPassword = _configuration["Email:Password"];
                var emailFrom = _configuration["Email:From"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Pegasus App", emailFrom));
                message.To.Add(new MailboxAddress("", destinatario));
                message.Subject = asunto;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = contenido
                };

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Correo enviado a {destinatario}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error enviando correo a {destinatario}: {ex.Message}");
                throw;
            }
        }
    }
}