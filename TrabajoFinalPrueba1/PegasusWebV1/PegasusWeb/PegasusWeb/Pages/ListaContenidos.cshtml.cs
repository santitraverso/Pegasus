using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ListaContenidosModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<ContenidoMaterias> Contenidos { get; set; } = new List<ContenidoMaterias> { };

        [TempData]
        public int IdMateria { get; set; }
        [TempData]
        public int IdContenido { get; set; }
        public async Task OnGetAsync()
        {
            Contenidos = await GetContenidosAsync(IdMateria);
        }

        static async Task<List<ContenidoMaterias>> GetContenidosAsync(int materia)
        {
            List<ContenidoMaterias> getcontenidos = new List<ContenidoMaterias>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_materia=={materia}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/ContenidoMaterias/GetContenidoMateriasForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string contenidosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(contenidosJson))
                {
                    getcontenidos = JsonConvert.DeserializeObject<List<ContenidoMaterias>>(contenidosJson);
                }
            }

            return getcontenidos;
        }

        public async Task<IActionResult> OnPostAsync(int contenido, bool editar)
        {
            if (editar)
            {
                IdContenido = contenido;
                return RedirectToPage("../CreateContenidoMateria");
            }
            else
            {
                return Page();
            }
            
        }

        public IActionResult OnPostAgregarContenido(int materia)
        {
            IdMateria = materia;
            return RedirectToPage("../CreateContenidoMateria");
        }
    }
}
