using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Reflection;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class CalificacionModel : PageModel
    {
        static HttpClient client = new HttpClient();

        public List<IntegrantesMaterias> Alumnos { get; set; } = new List<IntegrantesMaterias>();

        [TempData]
        public int Materia { get; set; }
        [TempData]
        public int IdIntegrante { get; set; }
        [TempData]
        public bool Nuevo { get; set; }
        [TempData]
        public int IdCurso { get; set; }

        [TempData]
        public string Modulo { get; set; }

        public async Task OnGetAsync()
        {

            var alumnos = await GetIntegrantesCursosAsync(IdCurso);

            // Crea una lista para almacenar las tareas
            var tasks = alumnos.Select(async alumno =>
            {
                // Crea un nuevo objeto IntegrantesMaterias
                IntegrantesMaterias inte = new IntegrantesMaterias
                {
                    Id_Materia = this.Materia,
                    Id_Usuario = alumno.Id_Usuario,
                    Usuario = alumno.Usuario
                };

                inte.Usuario.Calificaciones = await GetCalificacionesAsync(this.Materia, IdCurso, (int)inte.Id_Usuario);

                return inte;
            });

            // Espera a que todas las tareas se completen y agrega los resultados a la lista
            Alumnos.AddRange(await Task.WhenAll(tasks));
        }

        public IActionResult OnPost(int usuario, int materia, bool nuevo, int curso, string modulo)
        {
            IdIntegrante = usuario;
            Materia = materia;
            Nuevo = nuevo;
            IdCurso = curso;
            Modulo = modulo;
            return RedirectToPage("CreateCalificacion");
        }

        public IActionResult OnPostAtras(int curso, string modulo)
        {
            IdCurso = curso;
            Modulo = modulo;
            return RedirectToPage("Materia/ListaMaterias");
        }

        public IActionResult OnPostReporte(int materia, int curso, string modulo)
        {
            Materia = materia;
            IdCurso = curso;
            Modulo = modulo;
            return RedirectToPage("ReporteCalificaciones");
        }

        public static async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso)
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

        public static async Task<List<Calificaciones>> GetCalificacionesAsync(int materia, int curso, int usuario)
        {
            List<Calificaciones> getCalificaciones = new List<Calificaciones>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_materia=={materia} && x.id_curso=={curso} && x.id_alumno=={usuario}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Calificaciones/GetCalificacionesForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string alumnosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnosJson))
                {
                    getCalificaciones = JsonConvert.DeserializeObject<List<Calificaciones>>(alumnosJson);
                }
            }

            return getCalificaciones;
        }
    }
}
