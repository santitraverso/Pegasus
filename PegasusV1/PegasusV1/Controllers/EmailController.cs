using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using PegasusV1.Request;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace PegasusV1.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IService<Usuario> _usuarioService;

        public EmailController(IEmailService emailService, IService<Usuario> usuarioService)
        {
            _emailService = emailService;
            _usuarioService = usuarioService;
        }

        [HttpPost]
        [Route("EnviarCorreoInformativo")]
        public async Task<IActionResult> EnviarCorreoInformativo([FromBody] EventoEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var validationResult = ValidateEmailRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { error = validationResult.ErrorMessage });
                }

                var usuarios = await GetUsuariosByTipoDestinatario(request.TipoDestinatario);

                if (!usuarios.Any())
                {
                    return BadRequest(new { error = "No se encontraron usuarios para el tipo de destinatario especificado" });
                }

                var emailsSent = 0;
                var emailsFailed = 0;

                foreach (var usuario in usuarios)
                {
                    if (IsValidEmail(usuario.Mail))
                    {
                        try
                        {
                            var emailContent = GenerarContenidoEmailInformativo(request, usuario);
                            await _emailService.EnviarEmailAsync(usuario.Mail,
                                $"Nuevo Evento: {SanitizeSubject(request.Nombre)}", emailContent);
                            emailsSent++;
                        }
                        catch
                        {
                            emailsFailed++;
                        }

                        await Task.Delay(100);
                    }
                    else
                    {
                        emailsFailed++;
                    }
                }

                return Ok(new
                {
                    message = "Proceso de envío completado",
                    emailsSent,
                    emailsFailed,
                    totalUsuarios = usuarios.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet("TestEmail")]
        public async Task<IActionResult> TestEmail([Required][EmailAddress] string destinatario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (!IsValidEmail(destinatario))
                {
                    return BadRequest(new { error = "Dirección de correo electrónico inválida" });
                }

                var contenido = GenerarContenidoPrueba();
                await _emailService.EnviarEmailAsync(destinatario, "Prueba de correo - Pegasus App", contenido);

                return Ok(new { message = $"Correo de prueba enviado a {destinatario}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        private async Task<List<Usuario>> GetUsuariosByTipoDestinatario(TipoDestinatario tipoDestinatario)
        {
            return tipoDestinatario switch
            {
                TipoDestinatario.Alumnos => await _usuarioService.GetForCombo(x => x.Id_Perfil == 2 && x.Activo == true),
                TipoDestinatario.Padres => await _usuarioService.GetForCombo(x => x.Id_Perfil == 4 && x.Activo == true),
                TipoDestinatario.Ambos => await _usuarioService.GetForCombo(x => (x.Id_Perfil == 2 || x.Id_Perfil == 4) && x.Activo == true),
                _ => new List<Usuario>()
            };
        }

        private (bool IsValid, string ErrorMessage) ValidateEmailRequest(EventoEmailRequest request)
        {
            if (request == null)
                return (false, "Request no puede ser nulo");

            if (string.IsNullOrWhiteSpace(request.Nombre))
                return (false, "El nombre del evento es requerido");

            if (request.Nombre.Length > 200)
                return (false, "El nombre del evento no puede exceder 200 caracteres");

            if (string.IsNullOrWhiteSpace(request.Descripcion))
                return (false, "La descripción del evento es requerida");

            if (request.Descripcion.Length > 1000)
                return (false, "La descripción no puede exceder 1000 caracteres");

            if (request.Fecha < DateTime.Now.Date)
                return (false, "La fecha del evento no puede ser anterior a hoy");

            if (!Enum.IsDefined(typeof(TipoDestinatario), request.TipoDestinatario))
                return (false, "Tipo de destinatario inválido");

            return (true, string.Empty);
        }

        private string GenerarContenidoEmailInformativo(EventoEmailRequest evento, Usuario usuario)
        {
            var nombreSanitizado = SanitizeHtml(evento.Nombre);
            var descripcionSanitizada = SanitizeHtml(evento.Descripcion);
            var nombreUsuario = SanitizeHtml(usuario.Nombre ?? "Usuario");

            var html = $@"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px;'>
                    <h2 style='color: #333; text-align: center; margin-bottom: 20px;'>Nuevo Evento: {nombreSanitizado}</h2>
                    <p><strong>Estimado/a {nombreUsuario},</strong></p>
                    <p>Te informamos sobre el siguiente evento:</p>
                    <div style='background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                        <p><strong>Fecha:</strong> {evento.Fecha:dd/MM/yyyy HH:mm}</p>
                        <p><strong>Descripción:</strong> {descripcionSanitizada}</p>
                    </div>";

            if (evento.RequiereConfirmacion)
            {
                html += @"
                    <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; border-left: 4px solid #ffc107;'>
                        <p><strong>Este evento requiere confirmación de asistencia.</strong></p>
                        <p>Por favor, ingresa a la aplicación para confirmar tu asistencia.</p>
                    </div>";
            }

            html += @"
                    <p style='margin-top: 20px; font-size: 12px; color: #666;'>
                        Este es un mensaje automático, por favor no responder a este correo.
                    </p>
                </div>
            </body>
            </html>";

            return html;
        }

        private string GenerarContenidoPrueba()
        {
            return @"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px;'>
                    <h2 style='color: #333; text-align: center;'>Prueba de Correo - Pegasus App</h2>
                    <p>Este es un correo de prueba enviado desde Pegasus App.</p>
                    <p>Si recibiste este correo, la configuración de correo electrónico está funcionando correctamente.</p>
                </div>
            </body>
            </html>";
        }

        private bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email && email.Length <= 254;
            }
            catch
            {
                return false;
            }
        }

        private string SanitizeHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;")
                .Replace("&", "&amp;");
        }

        private string SanitizeSubject(string subject)
        {
            if (string.IsNullOrEmpty(subject))
                return "Sin asunto";

            return subject
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim()
                .Substring(0, Math.Min(subject.Length, 200));
        }
    }
}
