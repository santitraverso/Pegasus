using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class CuadernoModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<IntegrantesCursos> IntegrantesCurso { get; set; } = new List<IntegrantesCursos>();
        [TempData]
        public int IdCurso { get; set; }
        [TempData]
        public string Modulo { get; set; }

        [BindProperty]
        public List<int> SelectedAlumnosIds { get; set; } = new List<int>(); // IDs de los alumnos seleccionados en el formulario

        [TempData]
        public List<int> IdsAlumnos { get; set; }

        public async Task OnGetAsync()
        {
            IntegrantesCurso = await GetIntegrantesCursosAsync(IdCurso);
        }

        public static async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            string queryParam = Uri.EscapeDataString($"x=>x.id_curso == {curso}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/IntegrantesCursos/GetIntegrantesCursosForCombo?query={queryParam}");

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

        public async Task<IActionResult?> OnPost(int curso, bool ver, string modulo, bool atras)
        {
            IdCurso = curso;
            Modulo = modulo;

            if (atras)
                return RedirectToPage("ListaCursos");

            if (SelectedAlumnosIds.Count < 1)
            this.ModelState.AddModelError("cuaderno", "Debe seleccionar alumnos");

            IdsAlumnos = SelectedAlumnosIds;

            if (ver)
            {
                return RedirectToPage("ListaComunicados");
            }
            else
            {
                return RedirectToPage("CreateComunicado");
            }
          
           
            
        }

        
    }
}