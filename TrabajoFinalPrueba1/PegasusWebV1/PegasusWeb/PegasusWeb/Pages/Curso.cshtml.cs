using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class CursoModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Curso> Cursos { get; set; }

        [TempData]
        public int IdCurso { get; set; }

        public async Task OnGetAsync()
        {
            Cursos = await GetCursosAsync();
        }

        public async Task<IActionResult> OnPostAsync(int curso, bool editar)
        {
            IdCurso = curso;

            if (editar)
            {
                return RedirectToPage("CreateCurso");
            }
            else
            {
                await EliminarCursoAsync(curso);
                Cursos = await GetCursosAsync();
                return RedirectToPage("Curso");
            }
        }


        static async Task<List<Curso>> GetCursosAsync()
        {
            List<Curso> getcursos = new List<Curso>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Contactos/GetContactosForCombo");
            HttpResponseMessage response = await client.GetAsync("http://localhost:7130/Curso/GetCursosForCombo");
            if (response.IsSuccessStatusCode)
            {
                string cursosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursosJson))
                {
                    getcursos = JsonConvert.DeserializeObject<List<Curso>>(cursosJson);
                }
            }

            return getcursos;
        }

        public async Task EliminarCursoAsync(int curso)
        {
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Curso/DeleteCurso?id={curso}");

            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("curso", "Hubo un error inesperado al borrar el Curso");
            }
        }
    }

}
