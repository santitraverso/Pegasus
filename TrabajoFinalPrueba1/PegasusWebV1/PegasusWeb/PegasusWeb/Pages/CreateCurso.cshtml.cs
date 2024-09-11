using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class CreateCursoModel : PageModel
    {
        static HttpClient client = new HttpClient();

        [DisplayName("Nombre del curso"), Required(ErrorMessage = "El campo Nombre del Curso es requerido")]
        public string nombreCurso { get; set; }

        [DisplayName("Nombre de la materia"), Required(ErrorMessage = "El campo Nombre de la materia es requerido")]
        public string nombreMateria { get; set; }

        [DisplayName("Materia"), Required(ErrorMessage = "Hubo un error inesperado creando la Materia")]
        public string materia { get; set; }

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

        public async Task<IActionResult> OnPost(string curso, string nombreMateria)
        {
            if (string.IsNullOrEmpty(curso))
            {
                this.ModelState.AddModelError("curso", "El campo Curso es requerido");
                return null;
            }

            if (string.IsNullOrEmpty(nombreMateria))
            {
                this.ModelState.AddModelError("nombreMateria", "El campo Nombre de la materia es requerido");
                return null;
            }

            var content = new StringContent($"{{\"Nombre\":\"{nombreMateria}\", \"Curso\":\"{curso}\"}}", Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://pegasus.azure-api.net/v1/Materia/CreateMateria", content);
            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("materia", "Hubo un error inesperado al crear la Materia");
                return null;
            }

            return RedirectToPage("Curso");
        }
    }
}
