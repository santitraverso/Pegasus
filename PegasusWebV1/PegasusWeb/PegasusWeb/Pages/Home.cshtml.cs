using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;

namespace PegasusWeb.Pages
{
    public class HomeModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public HomeModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<ModulosPerfiles> Modulos { get; set; } = new List<ModulosPerfiles>();
        public Dictionary<string, object>? UsuarioData { get; set; } // Diccionario para almacenar los datos

        [TempData]
        public int IdPerfil { get; set; }

        [TempData]
        public int IdUsuario { get; set; }
        [TempData]
        public string? Modulo { get; set; }

        public async Task<IActionResult> OnGetAsync(string? usuario)
        {

            // Verificar primero si hay un token válido
            string token = HttpContext.Session.GetString("JwtToken") ?? "";

            if (!string.IsNullOrEmpty(usuario))
            {
                //Usuario viene del login con parámetros
                try
                {
                    usuario = Uri.UnescapeDataString(usuario);
                    UsuarioData = JsonConvert.DeserializeObject<Dictionary<string, object>>(usuario);

                    if (UsuarioData != null && UsuarioData.ContainsKey("token"))
                    {
                        token = UsuarioData["token"].ToString() ?? "";

                        // Verificar que el token sea válido antes de guardarlo
                        if (!string.IsNullOrEmpty(token) && await ValidateTokenAsync(token))
                        {
                            HttpContext.Session.SetString("JwtToken", token);
                            await SaveUserDataToSession(UsuarioData);
                        }
                        else
                        {
                            // Token inválido, limpiar y redirigir
                            HttpContext.Session.Clear();
                            return RedirectToPage("/Index");
                        }
                    }
                    else
                    {
                        // No hay token en los datos del usuario
                        HttpContext.Session.Clear();
                        return RedirectToPage("/Index");
                    }
                }
                catch
                {
                    // Error al procesar datos del usuario
                    HttpContext.Session.Clear();
                    return RedirectToPage("/Index");
                }
            }
            else
            {
                //Acceso directo a /Home
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/Index");
                }

                // Verificar que el token siga siendo válido
                bool isTokenValid = await ValidateTokenAsync(token);
                if (!isTokenValid)
                {
                    HttpContext.Session.Clear();
                    return RedirectToPage("/Index");
                }
            }

            // Si llegamos aquí, tenemos una sesión válida
            IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;
            IdUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            if (IdPerfil <= 0 || IdUsuario <= 0)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Index");
            }

            // Cargar módulos
            Modulos = await GetModulosPerfilAsync(IdPerfil);

            return Page();
        }

        private async Task SaveUserDataToSession(Dictionary<string, object> userData)
        {
            if (userData == null) return;

            // Guardar IdPerfil
            if (userData.ContainsKey("id_perfil") &&
                int.TryParse(userData["id_perfil"].ToString(), out int idPerfilValue) &&
                idPerfilValue > 0)
            {
                IdPerfil = idPerfilValue;
                HttpContext.Session.SetInt32("IdPerfil", IdPerfil);
            }

            // Guardar Perfil
            if (userData.ContainsKey("perfil") &&
                !string.IsNullOrEmpty(userData["perfil"]?.ToString()))
            {
                HttpContext.Session.SetString("Perfil", userData["perfil"].ToString());
            }

            // Guardar IdUsuario
            if (userData.ContainsKey("id") &&
                int.TryParse(userData["id"].ToString(), out int idValue) &&
                idValue > 0)
            {
                IdUsuario = idValue;
                HttpContext.Session.SetInt32("IdUsuario", IdUsuario);

                // Si es padre, obtener hijo
                if (IdPerfil == (int)TipoPerfil.Padre)
                {
                    var hijo = await GetHijosAsync(IdUsuario);
                    HttpContext.Session.SetInt32("IdHijo", hijo?.Id_Hijo ?? 0);
                }
            }

            // Guardar datos para JavaScript
            if (userData.ContainsKey("apellido"))
                HttpContext.Session.SetString("UserApellido", userData["apellido"].ToString() ?? "");

            if (userData.ContainsKey("nombre"))
                HttpContext.Session.SetString("UserNombre", userData["nombre"].ToString() ?? "");

            if (userData.ContainsKey("email"))
                HttpContext.Session.SetString("UserEmail", userData["email"].ToString() ?? "");
        }

        private async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                // Hacer una llamada simple a la API para verificar si el token es válido
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/ModulosPerfiles/GetModulosPerfilesForCombo?query=x%3D%3Ex.id_perfil%3D%3D1");
                request.Headers.Add("Authorization", $"Bearer {token}");

                HttpResponseMessage response = await _client.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ModulosPerfiles>> GetModulosPerfilAsync(int perfil)
        {
            List<ModulosPerfiles> getmodulos = new List<ModulosPerfiles>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_perfil=={perfil}");

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/ModulosPerfiles/GetModulosPerfilesForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string alumnosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnosJson))
                {
                    getmodulos = JsonConvert.DeserializeObject<List<ModulosPerfiles>>(alumnosJson);
                }
            }

            return getmodulos;
        }

        public async Task<Hijo> GetHijosAsync(int padre)
        {
            Hijo gethijo = new Hijo();

            string queryParam = Uri.EscapeDataString($"x=>x.id_padre=={padre}");

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Hijo/GetHijosForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string hijosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(hijosJson))
                {
                    gethijo = JsonConvert.DeserializeObject<List<Hijo>>(hijosJson).FirstOrDefault();
                }
            }

            return gethijo;
        }

        public async Task<IActionResult> OnPostAsync(int perfil, int usuario, string page, string parametro)
        {
            // Verificar sesión antes de procesar el POST
            string token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Index");
            }

            bool isTokenValid = await ValidateTokenAsync(token);
            if (!isTokenValid)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Index");
            }

            IdPerfil = perfil;
            IdUsuario = usuario;

            if(!string.IsNullOrEmpty(parametro))
            {
                Modulo = parametro;
            }


            if (IdPerfil == (int)TipoPerfil.Alumno || IdPerfil == (int)TipoPerfil.Padre)
            {
                switch (Modulo)
                {
                    case "Calificacion":
                    case "Asistencia":
                        return RedirectToPage("Materia/ListaMaterias");
                    case "Cuaderno":
                        return RedirectToPage("Cuaderno");
                    case "Desempenio":
                        return RedirectToPage("Desempenio");
                }
            }

            return RedirectToPage(page);

        }
    }
}