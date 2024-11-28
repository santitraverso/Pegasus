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
using System.Linq.Dynamic.Core.Tokenizer;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;
using System.Net.Http;

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
        private readonly HttpClient _httpClient;

        public AccountController(ILogger<AccountController> logger,
            IService<Usuario> usuarioService,
            IService<Perfiles> perfilesService,
            HttpClient httpClient)
        {
            _logger = logger;
            UsuarioService = usuarioService;
            PerfilesService = perfilesService;
            _httpClient = httpClient;
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
                // Si no hay principal autenticado, redirigir con un mensaje de error
                return RedirectToAction("Error", "Home", new { message = "Autenticación fallida. Por favor, intente nuevamente." });
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
                string urlError = returnUrl.Replace("Home", "Error");
                return Redirect($"{urlError}?message={Uri.EscapeDataString("Usuario no encontrado. Pongase en contacto con la institución.")}");
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

            // Generar el token JWT
            //var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.UTF8.GetBytes(_secretKey);
            //var jwtToken = tokenHandler.CreateToken(new SecurityTokenDescriptor
            //{
            //    Subject = new ClaimsIdentity(claims),
            //    Expires = DateTime.UtcNow.AddDays(7), // El token expirará en 7 días
            //    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            //});

            //var tokenString = tokenHandler.WriteToken(jwtToken);

            // Devolver los datos del usuario en formato JSON al frontend
            var userData = new
            {
                email = email,
                nombre = usuario?.Nombre,
                apellido = usuario?.Apellido,
                id = usuario?.Id,
                perfil = usuario?.Perfil?.Nombre,
                id_perfil = usuario?.Perfil?.Id
                //token = tokenString // Incluimos el JWT en la respuesta
            };

            // Convierte el objeto a una cadena JSON
            var userDataJson = JsonConvert.SerializeObject(userData);
            // Devuelve la redirección con los datos como parámetro
            return Redirect($"{returnUrl}?usuario={Uri.EscapeDataString(userDataJson)}");
        }

        //    [HttpGet("Login")]
        //    public IActionResult Login(string returnUrl = "/")
        //    {
        //        // La URL de Google para iniciar sesión
        //        var redirectUri = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
        //        var requestUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        //        var requestParams = new Dictionary<string, string>
        //    {
        //    { "client_id", "579011975819-tsmscfrfs7p4ai2n72rm5g9isvfqtubo.apps.googleusercontent.com" },
        //    { "redirect_uri", redirectUri },
        //    { "response_type", "code" },
        //    { "scope", "openid profile email" },
        //    { "state", returnUrl }
        //    };

        //        // Redirige al usuario a Google para iniciar sesión
        //        return Redirect(QueryHelpers.AddQueryString(requestUrl, requestParams));
        //    }

        //    [HttpGet("ExternalLoginCallback")]
        //    public async Task<IActionResult> ExternalLoginCallback(string code, string state)
        //    {
        //        var tokenResponse = await ExchangeCodeForTokens(code);
        //        var userInfo = await GetGoogleUserInfo(tokenResponse.AccessToken);

        //        // Iniciar sesión del usuario
        //        var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.Name, userInfo.Name),
        //    new Claim(ClaimTypes.Email, userInfo.Email)
        //};

        //        var identity = new ClaimsIdentity(claims, "Google");
        //        var principal = new ClaimsPrincipal(identity);
        //        await HttpContext.SignInAsync(principal);

        //        // Redirigir al usuario al returnUrl
        //        return Redirect(state);
        //    }

        //    private async Task<TokenResponse> ExchangeCodeForTokens(string code)
        //    {
        //        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        //        {
        //            Content = new FormUrlEncodedContent(new Dictionary<string, string>
        //    {
        //        { "code", code },
        //        { "client_id", "579011975819-tsmscfrfs7p4ai2n72rm5g9isvfqtubo.apps.googleusercontent.com" },
        //        { "client_secret", "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" },
        //        { "redirect_uri", "/Account/ExternalLoginCallback" },
        //                { "grant_type", "authorization_code" }
        //    })
        //        };

        //        var response = await _httpClient.SendAsync(tokenRequest);
        //        response.EnsureSuccessStatusCode();
        //        var tokenContent = await response.Content.ReadFromJsonAsync<TokenResponse>();
        //        return tokenContent;
        //    }

        //    private async Task<GoogleUserInfo> GetGoogleUserInfo(string accessToken)
        //    {
        //        var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
        //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //        var response = await _httpClient.SendAsync(request);
        //        response.EnsureSuccessStatusCode();
        //        var userInfo = await response.Content.ReadFromJsonAsync<GoogleUserInfo>();
        //        return userInfo;
        //    }

        //    public class TokenResponse
        //    {
        //        public string AccessToken { get; set; }
        //        public string IdToken { get; set; }
        //    }

        //    public class GoogleUserInfo
        //    {
        //        public string Name { get; set; }
        //        public string Email { get; set; }
        //    }
    }
}