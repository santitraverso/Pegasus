using PegasusV1.Entities;

namespace PegasusV1.Interfaces
{
    public interface IEmailService
    {
        Task EnviarEmailAsync(string destinatario, string asunto, string contenido);
    }
}
