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

        public async Task<IActionResult> OnGetAsync()
        {
            if (IdCurso > 0)
            {
                // Es una edición, se carga el curso existente
                HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Curso/GetById?id={IdCurso}");

                if (response.IsSuccessStatusCode)
                {
                    string cursoJson = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(cursoJson))
                    {
                        Curso = JsonConvert.DeserializeObject<Curso>(cursoJson);
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
                Curso = new Curso();
            }

            return Page();
        }

        public async Task<IActionResult> OnPost(int grado, string nombre, string division, string turno, int id)
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
                HttpResponseMessage response = await client.PutAsync("https://localhost:7130/Curso/UpdateCurso", content);
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
                HttpResponseMessage response = await client.PostAsync("https://localhost:7130/Curso/CreateCurso", content);
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
