using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using PegasusV1.Request;
using PegasusV1.Security;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IService<Usuario> _usuarioService;
        private readonly ILogger<AccountController> _logger;
        private readonly IService<Perfiles> _perfilesService;
        private readonly IService<ModulosPerfiles> _modulosPerfilesService;
        private readonly IConfiguration _configuration;
        private readonly JwtGenerator _jwtGenerator;

        public AccountController(
            ILogger<AccountController> logger,
            IService<Usuario> usuarioService,
            IService<Perfiles> perfilesService,
            IService<ModulosPerfiles> modulosPerfilesService,
            IConfiguration configuration,
            JwtGenerator jwtGenerator,
            HttpClient httpClient = null)
        {
            _logger = logger;
            _usuarioService = usuarioService;
            _perfilesService = perfilesService;
            _modulosPerfilesService = modulosPerfilesService;
            _configuration = configuration;
            _jwtGenerator = jwtGenerator;
        }

        [HttpGet("Login")]
        public IActionResult Login(string returnUrl = "/")
        {
            // Validación básica de seguridad para returnUrl
            if (!IsValidReturnUrl(returnUrl))
            {
                _logger.LogWarning("Intento de redirección potencialmente maliciosa bloqueado: {ReturnUrl}", returnUrl);
                returnUrl = "/";
            }

            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("Logout")]
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            // Validación básica de seguridad para returnUrl
            if (!IsValidReturnUrl(returnUrl))
            {
                _logger.LogWarning("Intento de redirección potencialmente maliciosa bloqueado: {ReturnUrl}", returnUrl);
                returnUrl = "/";
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect(returnUrl);
        }

        [HttpGet("ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
        {
            // Validación básica de seguridad para returnUrl
            if (!IsValidReturnUrl(returnUrl))
            {
                _logger.LogWarning("Intento de redirección potencialmente maliciosa bloqueado: {ReturnUrl}", returnUrl);
                returnUrl = "/";
            }

            try
            {
                var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (result?.Principal == null)
                {
                    return Redirect($"{returnUrl.Replace("Home", "Error")}?message=Autenticación fallida. Por favor, intente nuevamente.");
                }

                var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
                var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
                {
                    return BadRequest(new { message = "No se encontró el email en los claims." });
                }

                var usuario = await GetUsuarioByEmail(email);

                if (usuario == null)
                {
                    return Redirect($"{returnUrl.Replace("Home", "Error")}?message=Usuario no encontrado. Póngase en contacto con la institución.");
                }

                if (usuario.Id_Perfil.HasValue)
                {
                    usuario.Perfil = await _perfilesService.GetById(usuario.Id_Perfil.Value);
                }

                // Generar token JWT
                string token = _jwtGenerator.GenerateToken(usuario);

                var claimsIdentity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim("Perfil", usuario?.Perfil?.Nombre ?? ""),
                    new Claim("JwtToken", token)
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);


                var userData = new
                {
                    email = email,
                    nombre = usuario?.Nombre ?? "",
                    apellido = usuario?.Apellido ?? "",
                    id = usuario?.Id,
                    perfil = usuario?.Perfil?.Nombre ?? "",
                    id_perfil = usuario?.Perfil?.Id,
                    token = token
                };

                var userDataJson = JsonConvert.SerializeObject(userData);
                return Redirect($"{returnUrl}?usuario={Uri.EscapeDataString(userDataJson)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ExternalLoginCallback");
                return Redirect($"{returnUrl.Replace("Home", "Error")}?message=Error interno del servidor.");
            }
        }

        [HttpPost("loginApp")]
        public async Task<IActionResult> LoginApp([FromBody] LoginRequest request)
        {
            try
            {
                // Validar que el request tenga los datos necesarios
                if (request == null || string.IsNullOrEmpty(request.GoogleToken))
                {
                    _logger.LogWarning("Intento de login sin token de Google");
                    return BadRequest("Token de Google requerido");
                }

                // Validar el email
                if (string.IsNullOrEmpty(request.Email) || !IsValidEmail(request.Email))
                {
                    _logger.LogWarning("Intento de login con email inválido: {Email}", request.Email);
                    return BadRequest("Email inválido");
                }

                var googleUser = await ValidateGoogleToken(request.GoogleToken);
                if (googleUser == null)
                {
                    return Unauthorized("Token de Google inválido");
                }

                if (!string.Equals(googleUser.Email, request.Email, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("El email del token ({TokenEmail}) no coincide con el email solicitado ({RequestEmail})",
                        googleUser.Email, request.Email);
                    return BadRequest("El email del token no coincide con el email solicitado");
                }

                // Usar el método seguro para obtener el usuario
                var usuario = await GetUsuarioByEmail(request.Email);

                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado. Póngase en contacto con la institución.");
                }

                if (usuario.Id_Perfil.HasValue)
                {
                    usuario.Perfil = await _perfilesService.GetById(usuario.Id_Perfil.Value);
                }

                List<ModulosPerfiles> modulosPerfiles = await _modulosPerfilesService.GetModulosPerfilesForUser(x => x.Id_Perfil == usuario.Id_Perfil.Value);

                List<Modulos> modulos = modulosPerfiles
                    .Where(mp => mp.Modulo != null)
                    .Select(mp => mp.Modulo!)
                    .ToList();

                // Generar token JWT
                string token = _jwtGenerator.GenerateToken(usuario);

                var userData = new
                {
                    usuario = usuario,
                    perfil = usuario?.Perfil,
                    modulos = modulos,
                    google_id = googleUser.Subject,
                    verified_email = googleUser.EmailVerified,
                    token = token
                };

                return Ok(userData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en loginApp");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private async Task<GoogleJsonWebSignature.Payload?> ValidateGoogleToken(string token)
        {
            try
            {
                var clientId = _configuration["GoogleOAuth:ClientId"];
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
                return payload;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validando token de Google");
                return null;
            }
        }

        private async Task<Usuario?> GetUsuarioByEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
            {
                return null;
            }

            var usuarios = await _usuarioService.GetForCombo(u => u.Mail == email);
            return usuarios.FirstOrDefault();
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.Length <= 254;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidReturnUrl(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
                return false;

            if (returnUrl.Length > 500)
                return false;

            if (returnUrl.Contains("javascript:", StringComparison.OrdinalIgnoreCase))
                return false;

            if (returnUrl.Contains("data:", StringComparison.OrdinalIgnoreCase))
                return false;

            if (returnUrl.StartsWith("//"))
                return false;

            // Permitir URLs absolutas (necesario para el flujo actual)
            return true;
        }
    }
}
