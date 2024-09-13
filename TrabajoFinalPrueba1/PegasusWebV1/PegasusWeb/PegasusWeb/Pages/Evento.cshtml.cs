using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class EventoModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Evento> Eventos { get; set; }
        
        public async Task OnGetAsync()
        {
            Eventos = await GetEventosAsync();
        }

        static async Task<List<Evento>> GetEventosAsync()
        {
            List<Evento> geteventos = new List<Evento>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Evento/GetEventosForCombo");
            HttpResponseMessage response = await client.GetAsync("http://localhost:7130/Evento/GetEventosForCombo");
            if (response.IsSuccessStatusCode)
            {
                string eventosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(eventosJson))
                {
                    geteventos = JsonConvert.DeserializeObject<List<Evento>>(eventosJson);
                }
            }

            return geteventos;
        }
    }
}
