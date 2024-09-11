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

        public async Task<IActionResult> OnPostAsync(int curso)
        {
            IdCurso = curso;
            return RedirectToPage("CreateCurso");
        }

        static async Task<List<Curso>> GetCursosAsync()
        {
            List<Curso> getcursos = new List<Curso>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Contactos/GetContactosForCombo");
            HttpResponseMessage response = await client.GetAsync("https://localhost:7130/Curso/GetCursosForCombo");
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
    }
}
