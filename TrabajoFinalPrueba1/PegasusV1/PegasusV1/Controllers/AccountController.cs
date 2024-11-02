using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IService<Usuario> UsuarioService;
        private readonly ILogger<AccountController> _logger;
        private readonly IService<Perfiles> PerfilesService;
        private readonly string _secretKey = "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";  // Clave secreta del JWT

        public AccountController(ILogger<AccountController> logger,
            IService<Usuario> usuarioService,
            IService<Perfiles> perfilesService)
        {
            _logger = logger;
            UsuarioService = usuarioService;
            PerfilesService = perfilesService;
        }

        [HttpGet("Login")]
        public IActionResult Login(string returnUrl = "/")
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("Logout")]
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect($"{returnUrl}");
        }

        [HttpGet("ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (result?.Principal == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Aquí puedes obtener los datos del usuario autenticado
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "No se encontró el email en los claims." });
            }

            // Buscar el usuario en la base de datos
            Usuario usuario = (await UsuarioService.GetForCombo(u => u.Mail == email)).FirstOrDefault();
            if (usuario == null)
            {
                return StatusCode(500, new { message = "Usuario no encontrado" });
            }

            // Verificar el perfil del usuario
            if (usuario.Id_Perfil.HasValue)
            {
                usuario.Perfil = await PerfilesService.GetById(usuario.Id_Perfil.Value);
            }


            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, email),
                new Claim("Perfil", usuario?.Perfil?.Nombre)
            }, CookieAuthenticationDefaults.AuthenticationScheme);

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            // Devolver los datos del usuario en formato JSON al frontend
            var userData = new
            {
                email = email,
                nombre = usuario?.Nombre,
                apellido = usuario?.Apellido,
                id = usuario?.Id,
                perfil = usuario?.Perfil?.Nombre,
                id_perfil = usuario?.Perfil?.Id
            };

            // Convierte el objeto a una cadena JSON
            var userDataJson = JsonConvert.SerializeObject(userData);
            // Devuelve la redirección con los datos como parámetro
            return Redirect($"{returnUrl}?usuario={Uri.EscapeDataString(userDataJson)}");
        }

        //[AllowAnonymous]
        //[HttpPost("google/callback")]
        //public async Task<IActionResult> GoogleCallback([FromBody] string token)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(token))
        //        {
        //            return BadRequest("El token es requerido.");
        //        }

        //        _logger.LogInformation("Token recibido: " + token);

        //        // Verificar el token con Google
        //        var payload = await GoogleJsonWebSignature.ValidateAsync(token);
        //        if (payload == null)
        //        {
        //            _logger.LogWarning("Token no válido.");
        //            return StatusCode(500, new { message = "Token no valido" });

        //        }

        //        _logger.LogInformation("Token validado con éxito.");

        //        // Obtener el email del usuario del token
        //        var email = payload.Email;

        //        // Buscar el usuario en la base de datos
        //        Usuario usuario = (await UsuarioService.GetForCombo(u => u.Mail == email)).FirstOrDefault();
        //        if (usuario == null)
        //        {
        //            return StatusCode(500, new { message = "Usuario no encontrado"});
        //        }

        //        // Verificar el perfil del usuario
        //        if (usuario.Id_Perfil.HasValue)
        //        {
        //            usuario.Perfil = await PerfilesService.GetById(usuario.Id_Perfil.Value);
        //        }

        //        // Crear los claims del JWT
        //        var claims = new[]
        //        {
        //            new Claim(ClaimTypes.Name, email),
        //            new Claim("Perfil", usuario?.Perfil?.Nombre ?? string.Empty)
        //        };

        //        // Generar el token JWT
        //        var tokenHandler = new JwtSecurityTokenHandler();
        //        var key = Encoding.UTF8.GetBytes(_secretKey);
        //        var jwtToken = tokenHandler.CreateToken(new SecurityTokenDescriptor
        //        {
        //            Subject = new ClaimsIdentity(claims),
        //            Expires = DateTime.UtcNow.AddDays(7), // El token expirará en 7 días
        //            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        //        });

        //        var tokenString = tokenHandler.WriteToken(jwtToken);

        //        // Devolver el token JWT
        //        return Ok(new { message = "Autenticación exitosa", Token = tokenString });

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al validar el token de Google.");
        //        return StatusCode(500, new { message = "Error en la autenticación", error = ex.Message });
        //    }
        //}
    }
}