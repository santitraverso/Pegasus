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
    public class CreateCursoModel : PageModel
    {
        static HttpClient client = new HttpClient();

        [DisplayName("Nombre"), Required(ErrorMessage = "El campo Nombre es requerido")]
        public string nombre { get; set; }

        [DisplayName("Grado"), Required(ErrorMessage = "El campo Grado es requerido")]
        public string grado { get; set; }

        [DisplayName("Division"), Required(ErrorMessage = "El campo Division es requerido")]
        public string division { get; set; }

        [DisplayName("Turno"), Required(ErrorMessage = "El campo Turno es requerido")]
        public string turno { get; set; }

        [BindProperty]
        public Curso Curso { get; set; }

        [TempData]
        public int IdCurso { get; set; }

        public List<IntegrantesCursos> Alumnos { get; set; } = new List<IntegrantesCursos>();

        public async Task<IActionResult> OnGetAsync()
        {
            if (IdCurso > 0)
            {
                // Es una edici�n, se carga el curso existente
                HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Curso/GetById?id={IdCurso}");

                if (response.IsSuccessStatusCode)
                {
                    string cursoJson = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(cursoJson))
                    {
                        Curso = JsonConvert.DeserializeObject<Curso>(cursoJson);

                        Alumnos = await GetIntegrantesCursosAsync(IdCurso);
                    }
                }

                if (Curso == null)
                {
                    return NotFound();
                }
            }
            else
            {
                // Es una carga nueva
                Curso = new Curso { Id = 0 };
            }

            return Page();
        }

        public IActionResult OnPostAgregarAlumno(int curso)
        {
            IdCurso = curso;
            return RedirectToPage("IntegrantesCursos");
        }

        static async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            string queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso}");
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

        public async Task<IActionResult> OnPostAsync(int grado, string nombre, string division, string turno, int id)
        {
            if (grado < 1)
            {
                this.ModelState.AddModelError("grado", "El campo Grado es requerido");
                return null;
            }
            if (string.IsNullOrEmpty(nombre))
            {
                this.ModelState.AddModelError("nombre", "El campo Curso es requerido");
                return null;
            }
            if (string.IsNullOrEmpty(division))
            {
                this.ModelState.AddModelError("division", "El campo Division es requerido");
                return null;
            }
            if (string.IsNullOrEmpty(turno))
            {
                this.ModelState.AddModelError("turno", "El campo Turno es requerido");
                return null;
            }

            if (id > 0)
            {
                // Actualizar curso existente
                var content = new StringContent($"{{\"Nombre_Curso\":\"{nombre}\", \"Grado\":\"{grado}\", \"Division\":\"{division}\", \"Turno\":\"{turno}\", \"Id\":\"{id}\"}}", Encoding.UTF8, "application/json");
                //HttpResponseMessage response = await client.PutAsync("https://pegasus.azure-api.net/v1/Curso/UpdateCurso", content);
                HttpResponseMessage response = await client.PutAsync("http://localhost:7130/Curso/UpdateCurso", content);
                if (!response.IsSuccessStatusCode)
                {
                    this.ModelState.AddModelError("curso", "Hubo un error inesperado al actualizar el Curso");
                    return null;
                }               
                
            }
            else
            {
                // Crear nuevo curso
                var content = new StringContent($"{{\"Nombre_Curso\":\"{nombre}\", \"Grado\":\"{grado}\", \"Division\":\"{division}\", \"Turno\":\"{turno}\"}}", Encoding.UTF8, "application/json");
                //HttpResponseMessage response = await client.PostAsync("https://pegasus.azure-api.net/v1/Materia/CreateMateria", content);
                HttpResponseMessage response = await client.PostAsync("http://localhost:7130/Curso/CreateCurso", content);
                if (!response.IsSuccessStatusCode)
                {
                    this.ModelState.AddModelError("curso", "Hubo un error inesperado al crear el Curso");
                    return null;
                }
            }

            return RedirectToPage("Curso");
        }
    }
}
