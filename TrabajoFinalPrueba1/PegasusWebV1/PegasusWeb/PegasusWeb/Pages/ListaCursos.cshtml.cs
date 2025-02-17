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
        public List<IntegrantesCursos> Cursos { get; set; }

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

            switch (IdPerfil)
            {
                //Alumno
                case 2:
                    Cursos = await GetCursosAsync(IdUsuario);
                    break;
                //Docente
                case 3:
                    var cursosDocente = await GetCursosDocenteAsync(IdUsuario);
                    Cursos = cursosDocente
                        .GroupBy(curso => curso.Id_Curso)
                        .Select(group => group.First())
                        .Select(curso => new IntegrantesCursos
                        {
                            Usuario = curso.Docente,
                            Id_Usuario = curso.Id_Docente,
                            Curso = curso.Curso,
                            Id_Curso = curso.Id_Curso,
                        })
                        .ToList();
                    break;
                //Padre
                case 4:
                    Cursos = await GetCursosAsync(HttpContext.Session.GetInt32("IdHijo") ?? IdUsuario);
                    break;

                default:
                    var cursos = await GetCursosAsync();
                    Cursos = cursos
                        .GroupBy(ic => ic.Id_Curso)
                        .Select(g => g.First())
                        .OrderBy(c => c.Id_Curso)
                        .ToList();
                    break;
            }
        }

        static async Task<List<IntegrantesCursos>> GetCursosAsync(int usuario = 0)
        {
            List<IntegrantesCursos> getcursos = new List<IntegrantesCursos>();
            HttpResponseMessage response;

            if (usuario > 0)
            {
                string queryParam = Uri.EscapeDataString($"x=>x.id_usuario=={usuario}");
                response = await client.GetAsync($"https://localhost:7130/IntegrantesCursos/GetIntegrantesCursosForCombo?query={queryParam}");
            }
            else
            {
                response = await client.GetAsync($"https://localhost:7130/IntegrantesCursos/GetIntegrantesCursosForCombo");
            }
           
            if (response.IsSuccessStatusCode)
            {
                string cursosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursosJson))
                {
                    getcursos = JsonConvert.DeserializeObject<List<IntegrantesCursos>>(cursosJson);
                }
            }

            return getcursos;
        }

        public static async Task<List<DocenteMateria>> GetCursosDocenteAsync(int docente)
        {
            List<DocenteMateria> getcursos = new List<DocenteMateria>();
            string queryParam = Uri.EscapeDataString($"x=>x.id_docente=={docente}");
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

        public IActionResult OnPost(int curso, string modulo, int usuario, int perfil)
        {
            IdCurso = curso;
            Modulo = modulo;
            IdUsuario = usuario;
            IdPerfil = perfil;

            switch (modulo)
            {
                case "Cuaderno":
                    return RedirectToPage("Cuaderno");
                case "Desempenio":
                    return RedirectToPage("Desempenio");
                default:
                    return RedirectToPage("Materia/ListaMaterias");
            }

        }
    }
}
