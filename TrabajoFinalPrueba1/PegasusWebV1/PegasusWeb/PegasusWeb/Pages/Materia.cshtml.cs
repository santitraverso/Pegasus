using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class MateriaModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Entities.Materia> Materias { get; set; }

        [TempData]
        public int IdMateria { get; set; }

        public async Task OnGetAsync()
        {
            Materias = await GetMateriasAsync();
        }

        public async Task<IActionResult> OnPostAsync(int materia, bool editar)
        {
            IdMateria = materia;
   
            if (editar)
            {
                return RedirectToPage("Materia/CreateMateria");
            }
            else
            {
                await EliminarMateriaAsync(materia);
                Materias = await GetMateriasAsync();
                return RedirectToPage("Materia");
            }
        }

        static async Task<List<Entities.Materia>> GetMateriasAsync()
        {
            List <Entities.Materia> getMaterias = new List<Entities.Materia>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Contactos/GetContactosForCombo");
            HttpResponseMessage response = await client.GetAsync("http://localhost:7130/Materia/GetMateriasForCombo");
            if (response.IsSuccessStatusCode)
            {
                string materiasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(materiasJson))
                {
                    getMaterias = JsonConvert.DeserializeObject<List<Entities.Materia>>(materiasJson);
                }
            }

            return getMaterias;
        }

        public async Task EliminarMateriaAsync(int materia)
        {
            HttpResponseMessage response = await client.DeleteAsync($"http://localhost:7130/Materia/DeleteMateria?id={materia}");

            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("materia", "Hubo un error inesperado al borrar la Materia");
            }
        }
    }
}
