using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages.Materia
{
    public class ListaMateriasModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<CursoMateria> Materias { get; set; } = new List<CursoMateria> { };

        [TempData]
        public int Materia { get; set; }

        [TempData]
        public int IdCurso { get; set; }

        [TempData]
        public string Modulo { get; set; }

        [TempData]
        public int IdUsuario { get; set; }

        [TempData]
        public int IdPerfil { get; set; }

        public async Task OnGetAsync()
        {

            IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;
            IdUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            if(IdPerfil == 3)
            {
                var materiasDocente = await GetMateriasDocenteAsync(IdUsuario, IdCurso);
                Materias = materiasDocente
                    .GroupBy(materia => materia.Id_Materia)
                    .Select(group => group.First())
                    .Select(materia => new CursoMateria
                    {
                        Curso = materia.Curso,
                        Id_Curso = materia.Id_Curso,
                        Materia = materia.Materia,
                        Id_Materia = materia.Id_Materia,
                    })
                    .ToList();
            }
            else
            {
                Materias = await GetMateriasAsync(IdCurso);
            }
        }

        static async Task<List<CursoMateria>> GetMateriasAsync(int curso)
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

            return getmaterias;
        }

        public static async Task<List<DocenteMateria>> GetMateriasDocenteAsync(int docente, int curso)
        {
            List<DocenteMateria> getcursos = new List<DocenteMateria>();
            string queryParam = Uri.EscapeDataString($"x=>x.id_docente=={docente} && x.id_curso=={curso}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/DocenteMateria/GetDocenteMateriaForCombo?query={queryParam}");
            

            if (response.IsSuccessStatusCode)
            {
                string cursosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursosJson))
                {
                    getcursos = JsonConvert.DeserializeObject<List<DocenteMateria>>(cursosJson);
                }
            }

            return getcursos;
        }

        public async Task<IActionResult> OnPostAsync(int materia, string modulo, int curso)
        {
            Materia = materia;
            Modulo = modulo;
            IdCurso = curso;

            switch (modulo)
            {
                case "Calificacion":
                    return RedirectToPage("../Calificacion");
                case "Asistencia":
                    return RedirectToPage("../Asistencia");
                default: 
                    return RedirectToPage("../Index");
            }
        }
    }
}
