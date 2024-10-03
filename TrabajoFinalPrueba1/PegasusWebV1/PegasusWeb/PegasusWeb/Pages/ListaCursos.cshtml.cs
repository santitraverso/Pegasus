using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ListaCursosModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Curso> Cursos { get; set; }

        [TempData]
        public int IdCurso { get; set; }

        [TempData]
        public string Modulo { get; set; }

        public async Task OnGetAsync(string modulo)
        {
            if(!string.IsNullOrEmpty(modulo))
                Modulo = modulo;
            Cursos = await GetCursosAsync();
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

        public IActionResult OnPost(int curso, string modulo)
        {
            IdCurso = curso;
            Modulo = modulo;

            switch (modulo)
            {
                case "Cuaderno":
                    return RedirectToPage("Cuaderno");
                case "Desempeno":
                    return RedirectToPage("Desempeno");
                default:
                    return RedirectToPage("Materia/ListaMaterias");
            }

        }
    }
}
