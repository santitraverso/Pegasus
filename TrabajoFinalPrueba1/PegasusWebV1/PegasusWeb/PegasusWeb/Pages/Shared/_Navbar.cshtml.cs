using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Reflection;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class NavbarModel: PageModel
    {
        static HttpClient client = new HttpClient();

        public List<ModulosPerfiles> Modulos { get; set; } = new List<ModulosPerfiles>();

        [TempData]
        public int IdPerfil { get; set; }

        public async Task OnGetAsync(int? perfil)
        {
            perfil = 1;
            if (perfil > 0)
            {
                IdPerfil = perfil.Value;
                Modulos = await GetModulosPerfilAsync(IdPerfil);
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
    }
}
