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

        public async Task OnGetAsync(string? usuario)
        {
            if (!string.IsNullOrEmpty(usuario))
            {
                usuario = Uri.UnescapeDataString(usuario);
                UsuarioData = JsonConvert.DeserializeObject<Dictionary<string, object>>(usuario);

                //Guarda el token en la sesión
                if (UsuarioData != null && UsuarioData.ContainsKey("token"))
                {
                    string token = UsuarioData["token"].ToString();
                    HttpContext.Session.SetString("JwtToken", token);
                }

                // Guardar IdPerfil en la sesión
                if (UsuarioData != null && UsuarioData.ContainsKey("id_perfil"))
                {
                    if (int.TryParse(UsuarioData["id_perfil"].ToString(), out int idPerfilValue) && idPerfilValue > 0)
                    {
                        IdPerfil = idPerfilValue;
                        HttpContext.Session.SetInt32("IdPerfil", IdPerfil);
                        Modulos = await GetModulosPerfilAsync(IdPerfil);
                    }
                }

                // Guardar Perfil en la sesión
                if (UsuarioData != null && UsuarioData.ContainsKey("perfil"))
                {
                    if (!string.IsNullOrEmpty(UsuarioData["perfil"]?.ToString()))
                    {
                        string perfil = UsuarioData["perfil"].ToString();
                        HttpContext.Session.SetString("Perfil", perfil);
                    }
                }

                // Guardar IdUsuario en la sesión
                if (UsuarioData != null && UsuarioData.ContainsKey("id"))
                {
                    if (int.TryParse(UsuarioData["id"].ToString(), out int idValue) && idValue > 0)
                    {
                        IdUsuario = idValue;
                        HttpContext.Session.SetInt32("IdUsuario", IdUsuario);

                        if (IdPerfil == (int)TipoPerfil.Padre)
                        {
                            var hijo = await GetHijosAsync(IdUsuario);
                            HttpContext.Session.SetInt32("IdHijo", hijo != null ? (int)hijo.Id_Hijo: 0);
                        }
                    }
                }
            }
            else
            {

                IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;
                if (IdPerfil > 0)
                {
                    Modulos = await GetModulosPerfilAsync(IdPerfil);
                }

                IdUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
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