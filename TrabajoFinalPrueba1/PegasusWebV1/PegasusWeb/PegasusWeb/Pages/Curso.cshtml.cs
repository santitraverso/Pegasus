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
                var tieneIntegrantes = await TieneIntegrantesCurso(curso);
                var tieneMaterias = await TieneMateriasCurso(curso);

                if (tieneIntegrantes)
                {
                    ModelState.AddModelError("curso", "No se puede eliminar el curso. Primero elimine los integrantes asociados.");
                    Cursos = await GetCursosAsync();
                    return Page();
                }
                else if (tieneMaterias)
                {
                    ModelState.AddModelError("curso", "No se puede eliminar el curso. Primero elimine las materias asociadas.");
                    Cursos = await GetCursosAsync();
                    return Page();
                }
                else
                {
                    await EliminarCursoAsync(curso);
                }
                     
                Cursos = await GetCursosAsync();
                return RedirectToPage("Curso");
            }
        }

        private async Task<bool> TieneIntegrantesCurso(int curso)
        {
            List<IntegrantesCursos> getintegrantes = new List<IntegrantesCursos>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso == {curso}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/IntegrantesCursos/GetIntegrantesCursosForCombo?query={queryParam}");
            if (response.IsSuccessStatusCode)
            {
                string integrantesJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(integrantesJson))
                {
                    getintegrantes = JsonConvert.DeserializeObject<List<IntegrantesCursos>>(integrantesJson);
                }
            }

            return getintegrantes.Count > 0;
        }

        private async Task<bool> TieneMateriasCurso(int curso)
        {
            List<CursoMateria> getmaterias = new List<CursoMateria>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso == {curso}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/CursoMateria/GetCursoMateriaForCombo?query={queryParam}");
            if (response.IsSuccessStatusCode)
            {
                string materiasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(materiasJson))
                {
                    getmaterias = JsonConvert.DeserializeObject<List<CursoMateria>>(materiasJson);
                }
            }

            return getmaterias.Count > 0;
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

        public async Task EliminarCursoAsync(int curso)
        {
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Curso/DeleteCurso?id={curso}");

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                this.ModelState.AddModelError("curso", "Hubo un error inesperado al borrar el Curso");
            }
        }
    }

}
