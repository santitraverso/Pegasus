using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http;

namespace PegasusWeb.Pages
{
    public class CreateContenidoMateriasModel : PageModel
    {
        static HttpClient client = new HttpClient();

        [BindProperty]
        public ContenidoMaterias Contenido { get; set; }

        [TempData]
        public int IdContenido { get; set; }
        [TempData]
        public int IdMateria { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (IdContenido > 0)
            {

                Contenido = await GetContenidoMateriasAsync(IdContenido);

                if (Contenido == null)
                {
                    return NotFound();
                }
            }
            else
            {
                Contenido = new ContenidoMaterias { Id = 0 };
            }

            return Page();
        }

        static async Task<ContenidoMaterias> GetContenidoMateriasAsync(int contenido)
        {
            ContenidoMaterias getcontenido = new ContenidoMaterias();

            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/ContenidoMaterias/GetById?id={contenido}");

            if (response.IsSuccessStatusCode)
            {
                string contenidoJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(contenidoJson))
                {
                    getcontenido = JsonConvert.DeserializeObject<ContenidoMaterias>(contenidoJson);
                }
            }

            return getcontenido;
        }


        public async Task<IActionResult> OnPostAsync(int materia, string titulo, string descripcion, int id)
        {
            if (materia < 1)
            {
                this.ModelState.AddModelError("materia", "El campo Materia es requerido");
                return null;
            }
            if (string.IsNullOrEmpty(titulo))
            {
                this.ModelState.AddModelError("titulo", "El campo Titulo es requerido");
                return null;
            }
            if (string.IsNullOrEmpty(descripcion))
            {
                this.ModelState.AddModelError("descripcion", "El campo Descripcion es requerido");
                return null;
            }

            if (id > 0)
            {
                var content = new StringContent($"{{\"Id_Materia\":\"{materia}\", \"Titulo\":\"{titulo}\", \"Descripcion\":\"{descripcion}\", \"Id\":\"{id}\"}}", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("http://localhost:7130/ContenidoMaterias/UpdateContenidoMaterias", content);
                if (!response.IsSuccessStatusCode)
                {
                    this.ModelState.AddModelError("contenido", "Hubo un error inesperado al actualizar el Contenido");
                    return null;
                }               
                
            }
            else
            {
                var content = new StringContent($"{{\"Id_Materia\":\"{materia}\", \"Titulo\":\"{titulo}\", \"Descripcion\":\"{descripcion}\"}}", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("http://localhost:7130/ContenidoMaterias/CreateContenidoMaterias", content);
                if (!response.IsSuccessStatusCode)
                {
                    this.ModelState.AddModelError("contenido", "Hubo un error inesperado al crear el Contenido");
                    return null;
                }
            }

            IdMateria = materia;
            return RedirectToPage("ListaContenidos");
        }
    }
}
