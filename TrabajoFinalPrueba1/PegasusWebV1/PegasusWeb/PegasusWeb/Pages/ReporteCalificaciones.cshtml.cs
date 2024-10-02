using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Dynamic;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ReporteCalificacionesModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<IntegrantesMaterias> Alumnos { get; set; }

        [TempData]
        public int Materia { get; set; }

        [BindProperty]
        public string Nombre { get; set; }

        public async Task OnGetAsync()
        {
            Alumnos = await GetIntegrantesMateriasAsync(Materia);
            var materia = await GetNombreMateriaAsync(Materia);
            Nombre = materia.Nombre;
        }


        public IActionResult OnPostAtras(int materia)
        {
            Materia = materia;
            return RedirectToPage("Calificacion");
        }

        static async Task<List<IntegrantesMaterias>> GetIntegrantesMateriasAsync(int materia)
        {
            List<IntegrantesMaterias> getalumnos = new List<IntegrantesMaterias>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            string queryParam = Uri.EscapeDataString($"x=>x.id_materia=={materia}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/IntegrantesMaterias/GetIntegrantesMateriasForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string alumnosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnosJson))
                {
                    getalumnos = JsonConvert.DeserializeObject<List<IntegrantesMaterias>>(alumnosJson);
                }
            }

            return getalumnos;
        }

        static async Task<Entities.Materia> GetNombreMateriaAsync(int materia)
        {
            Entities.Materia getMateria = new Entities.Materia();

            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Materia/GetById?id={materia}");

            if (response.IsSuccessStatusCode)
            {
                string materiaJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(materiaJson))
                {
                    getMateria = JsonConvert.DeserializeObject<Entities.Materia>(materiaJson);
                }
            }

            return getMateria;
        }
    }

}
