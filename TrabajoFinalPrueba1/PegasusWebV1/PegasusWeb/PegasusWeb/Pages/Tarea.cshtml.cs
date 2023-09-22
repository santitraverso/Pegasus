using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class TareaModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Tarea> Tareas { get; set; }
        
        public async Task OnGetAsync()
        {
            Tareas = await GetTareasAsync();
        }

        static async Task<List<Tarea>> GetTareasAsync()
        {
            List<Tarea> gettareas = new List<Tarea>();

            HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Tarea/GetTareasForCombo");
            if (response.IsSuccessStatusCode)
            {
                string tareasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(tareasJson))
                {
                    gettareas = JsonConvert.DeserializeObject<List<Tarea>>(tareasJson);
                }
            }

            return gettareas;
        }
    }
}
