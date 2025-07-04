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
                    var mensaje = "Usuario no encontrado. Póngase en contacto con la institución.";
                    var mensajeCodificado = Uri.EscapeDataString(mensaje);
                    return Redirect($"{returnUrl.Replace("Home", "Error")}?message={mensajeCodificado}");
                }

                if (usuario.Activo != true)
                {
                    var mensaje = "Usuario inactivo. Contacte al administrador.";
                    var mensajeCodificado = Uri.EscapeDataString(mensaje);
                    return Redirect($"{returnUrl.Replace("Home", "Error")}?message={mensajeCodificado}");

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
                _logger.LogInformation("=== INICIO LOGIN APP ===");
                _logger.LogInformation("Email: {Email}", request?.Email);
                _logger.LogInformation("Token presente: {HasToken}", !string.IsNullOrEmpty(request?.GoogleToken));
                _logger.LogInformation("Token length: {TokenLength}", request?.GoogleToken?.Length ?? 0);
                _logger.LogInformation("User-Agent: {UserAgent}", Request.Headers.UserAgent.ToString());
                _logger.LogInformation("Remote IP: {RemoteIP}", HttpContext.Connection.RemoteIpAddress?.ToString());

                // Validar que el request tenga los datos necesarios
                if (request == null || string.IsNullOrEmpty(request.GoogleToken))
                {
                    _logger.LogWarning("Intento de login sin token de Google");
                    return BadRequest(new { message = "Token de Google requerido", errorCode = "MISSING_TOKEN" });
                }

                // Validar el email
                if (string.IsNullOrEmpty(request.Email) || !IsValidEmail(request.Email))
                {
                    _logger.LogWarning("Intento de login con email inválido: {Email}", request.Email);
                    return BadRequest(new { message = "Email inválido", errorCode = "INVALID_EMAIL" });
                }

                _logger.LogInformation("Paso 1: Validando token de Google...");
                var googleUser = await ValidateGoogleToken(request.GoogleToken);

                if (googleUser == null)
                {
                    _logger.LogWarning("Token de Google inválido para email: {Email}", request.Email);
                    return Unauthorized(new { message = "Token de Google inválido", errorCode = "INVALID_GOOGLE_TOKEN" });
                }

                _logger.LogInformation("Paso 2: Token validado - Email: {TokenEmail}, Subject: {Subject}, EmailVerified: {EmailVerified}",
                    googleUser.Email, googleUser.Subject, googleUser.EmailVerified);

                if (!string.Equals(googleUser.Email, request.Email, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("El email del token ({TokenEmail}) no coincide con el email solicitado ({RequestEmail})",
                        googleUser.Email, request.Email);
                    return BadRequest(new { message = "El email del token no coincide con el email solicitado", errorCode = "EMAIL_MISMATCH" });
                }

                _logger.LogInformation("Paso 3: Buscando usuario en BD para email: {Email}...", request.Email);
                var usuario = await GetUsuarioByEmail(request.Email);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado en BD: {Email}", request.Email);
                    return NotFound(new { message = "Usuario no encontrado. Póngase en contacto con la institución.", errorCode = "USER_NOT_FOUND" });
                }

                _logger.LogInformation("Paso 4: Usuario encontrado - ID: {UserId}, Activo: {Activo}, Perfil: {PerfilId}",
                    usuario.Id, usuario.Activo, usuario.Id_Perfil);

                // Verificar si el usuario está activo
                if (usuario.Activo != true)
                {
                    _logger.LogWarning("Usuario inactivo intentó hacer login: {Email}", request.Email);
                    return Forbid(new { message = "Usuario inactivo. Contacte al administrador.", errorCode = "USER_INACTIVE" }.ToString());
                }

                _logger.LogInformation("Paso 5: Obteniendo perfil del usuario...");
                if (usuario.Id_Perfil.HasValue)
                {
                    usuario.Perfil = await _perfilesService.GetById(usuario.Id_Perfil.Value);
                    _logger.LogInformation("Perfil obtenido: {Perfil}", usuario.Perfil);
                }

                _logger.LogInformation("Paso 6: Obteniendo módulos del perfil...");
                List<ModulosPerfiles> modulosPerfiles = await _modulosPerfilesService.GetModulosPerfilesForUser(x => x.Id_Perfil == usuario.Id_Perfil.Value);
                _logger.LogInformation("Módulos encontrados: {ModulosCount}", modulosPerfiles?.Count ?? 0);

                List<Modulos> modulos = modulosPerfiles
                    .Where(mp => mp.Modulo != null)
                    .Select(mp => mp.Modulo!)
                    .ToList();

                _logger.LogInformation("Paso 7: Generando token JWT...");
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

                _logger.LogInformation("=== LOGIN EXITOSO para {Email} ===", request.Email);
                return Ok(userData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR DETALLADO en loginApp - Email: {Email}, Tipo: {ExceptionType}, Mensaje: {Message}, StackTrace: {StackTrace}",
                    request?.Email, ex.GetType().Name, ex.Message, ex.StackTrace);

                // Log adicional para errores específicos de Google
                if (ex.Message.Contains("Google") || ex.Message.Contains("token"))
                {
                    _logger.LogError("ERROR RELACIONADO CON GOOGLE TOKEN - Token Length: {TokenLength}, Email: {Email}",
                        request?.GoogleToken?.Length ?? 0, request?.Email);
                }

                return StatusCode(500, new { message = "Error interno del servidor", errorCode = "INTERNAL_ERROR" });
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
