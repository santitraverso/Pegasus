using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Dynamic;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ReporteCalificacionesModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<IntegrantesMaterias> Alumnos { get; set; } = new List<IntegrantesMaterias>();

        [TempData]
        public int Materia { get; set; }
        [TempData]
        public int IdCurso { get; set; }
        [BindProperty]
        public string Nombre { get; set; }
        [TempData]
        public string Modulo { get; set; }

        public async Task OnGetAsync()
        {
            //Alumnos = await GetIntegrantesMateriasAsync(Materia);

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

            var materia = await GetNombreMateriaAsync(Materia);
            Nombre = materia.Nombre;
        }


        public IActionResult OnPostAtras(int materia, int curso, string modulo)
        {
            Materia = materia;
            IdCurso = curso;
            Modulo = modulo;
            return RedirectToPage("Calificacion");
        }

        public static async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            string queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/IntegrantesCursos/GetIntegrantesCursosForCombo?query={queryParam}");

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
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Calificaciones/GetCalificacionesForCombo?query={queryParam}");

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

        static async Task<Entities.Materia> GetNombreMateriaAsync(int materia)
        {
            Entities.Materia getMateria = new Entities.Materia();

            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Materia/GetById?id={materia}");

            if (response.IsSuccessStatusCode)
            {
                string materiaJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(materiaJson))
                {
                    getMateria = JsonConvert.DeserializeObject<Entities.Materia>(materiaJson);
                }
            }

            return getMateria;
        }
    }

}
