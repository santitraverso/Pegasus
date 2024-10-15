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

        [TempData]
        public int IdEvento { get; set; }


        public async Task OnGetAsync()
        {
            var eventos = await GetEventosAsync();
            Eventos = eventos.OrderByDescending(e=> e.Fecha).ToList();
        }

        public static async Task<List<Evento>> GetEventosAsync()
        {
            List<Evento> geteventos = new List<Evento>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Evento/GetEventosForCombo");
            HttpResponseMessage response = await client.GetAsync("https://localhost:7130/Evento/GetEventosForCombo");
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

        public async Task<IActionResult> OnPostAsync(int evento, bool editar)
        {
            IdEvento= evento;

            if (editar)
            {
                return RedirectToPage("CreateEvento");
            }
            else
            {
                await EliminarEventoAsync(evento);
                await OnGetAsync();
                return RedirectToPage("Evento");
            }
        }

        public async Task EliminarEventoAsync(int evento)
        {
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Evento/DeleteEvento?id={evento}");

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                this.ModelState.AddModelError("evento", "Hubo un error inesperado al borrar el Evento");
            }
        }
    }
}
