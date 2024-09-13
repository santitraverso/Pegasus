using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class IntegrantesCursosModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<IntegrantesCursos> Alumnos { get; set; }
        
        [TempData]
        public int IdCurso { get; set; }

        [BindProperty]
        public List<int> SelectedAlumnosIds { get; set; } = new List<int>(); // IDs de los alumnos seleccionados en el formulario


        public async Task OnGetAsync()
        {
            var todos = await GetIntegrantesCursosAsync();

            Alumnos = todos
            .GroupBy(a => a.Id_Usuario)
            .Select(g => g.First())
            .ToList();

            foreach (var alumn in Alumnos)
            {
                if (alumn.Id_Curso == IdCurso)
                {
                    SelectedAlumnosIds.Add((int)alumn.Id_Usuario);
                }
            }
        }

        static async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync()
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            HttpResponseMessage response = await client.GetAsync("http://localhost:7130/IntegrantesCursos/GetIntegrantesCursosForCombo");

            if (response.IsSuccessStatusCode)
            {
                string alumnosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnosJson))
                {
                    getalumnos = JsonConvert.DeserializeObject<List<IntegrantesCursos>>(alumnosJson);
                }
            }

            return getalumnos;
        }
    }
}
