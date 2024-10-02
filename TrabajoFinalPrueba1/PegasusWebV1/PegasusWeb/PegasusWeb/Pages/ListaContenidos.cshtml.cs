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

        public async Task<IActionResult> OnPostAsync(int contenido, bool editar, int materia)
        {
            IdContenido = contenido;
            IdMateria = materia;
            
            if (editar)
            {
                return RedirectToPage("CreateContenidoMaterias");
            }
            else
            {
                await EliminarContenidoMateriasAsync(contenido);
                Contenidos = await GetContenidosAsync(IdMateria);
                return RedirectToPage("ListaContenidos");
            }
            
        }

        public IActionResult OnPostAgregarContenido(int materia, bool atras)
        {
            IdMateria = materia;

            if (atras)
            {
                return RedirectToPage("Materia/CreateMateria");
            }
            else
            {
                return RedirectToPage("CreateContenidoMaterias");
            }
            
        }

        public async Task EliminarContenidoMateriasAsync(int contenido)
        {
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/ContenidoMaterias/DeleteContenidoMaterias?id={contenido}");

            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("calificacion", "Hubo un error inesperado al borrar el Contenido");
            }
        }
    }
}
