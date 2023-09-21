using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ContenidoModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Contenido> Contenidos { get; set; }
        
        public async Task OnGetAsync()
        {
            Contenidos = await GetContenidosAsync();
        }

        static async Task<List<Contenido>> GetContenidosAsync()
        {
            List<Contenido> getcontenidos = new List<Contenido>();

            HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Contenido/GetContenidosForCombo");
            if (response.IsSuccessStatusCode)
            {
                string contenidosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(contenidosJson))
                {
                    getcontenidos = JsonConvert.DeserializeObject<List<Contenido>>(contenidosJson);
                }
            }

            return getcontenidos;
        }
    }
}
