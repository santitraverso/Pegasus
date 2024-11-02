using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;

namespace PegasusWeb.Pages
{
    public class HomeModel : PageModel
    {
        static HttpClient client = new HttpClient();

        public List<ModulosPerfiles> Modulos { get; set; } = new List<ModulosPerfiles>();
        public Dictionary<string, object> UsuarioData { get; set; } // Diccionario para almacenar los datos

        [TempData]
        public int IdPerfil { get; set; }

        [TempData]
        public int IdUsuario { get; set; }
        [TempData]
        public string Modulo { get; set; }

        public async Task OnGetAsync(string? usuario)
        {
            if (!string.IsNullOrEmpty(usuario))
            {
                usuario = Uri.UnescapeDataString(usuario);
                UsuarioData = JsonConvert.DeserializeObject<Dictionary<string, object>>(usuario);

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

        public static async Task<List<ModulosPerfiles>> GetModulosPerfilAsync(int perfil)
        {
            List<ModulosPerfiles> getmodulos = new List<ModulosPerfiles>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_perfil=={perfil}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/ModulosPerfiles/GetModulosPerfilesForCombo?query={queryParam}");

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

        public async Task<IActionResult> OnPostAsync(int perfil, int usuario, string page, string parametro)
        {
            IdPerfil = perfil;
            IdUsuario = usuario;

            if(!string.IsNullOrEmpty(parametro))
            {
                Modulo = parametro;
            }

            return RedirectToPage(page);
        }
    }
}