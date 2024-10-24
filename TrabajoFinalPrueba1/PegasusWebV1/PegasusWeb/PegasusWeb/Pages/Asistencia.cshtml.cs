using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Dynamic;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class AsistenciaModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Asistencia> Alumnos { get; set; } = new List<Asistencia>();
        public DateTime FechaAsistencia { get; set; } = DateTime.Now; // Fecha predeterminada, hoy

        [TempData]
        public int Materia { get; set; }

        [TempData]
        public DateTime Fecha { get; set; }

        [TempData]
        public int IdCurso { get; set; }

        [TempData]
        public string Modulo { get; set; }

        public async Task OnGetAsync()
        {
            if (Fecha != DateTime.MinValue)
                FechaAsistencia = Fecha;

            // Verificar si hay asistencia para el d�a actual
            if (!await ExisteAsistenciaParaFecha(Materia, FechaAsistencia, IdCurso))
            {
                // Si no hay asistencia, obtener todos los alumnos
                await GetAlumnosAsync(Materia);
            }
            else
            {
                // Si ya hay asistencia, cargar los alumnos con su asistencia
                Alumnos = await GetAsistenciaAsync(Materia, FechaAsistencia, IdCurso);
            }
        }

        public async Task<IActionResult> OnPost(DateTime fecha, int materia, int curso, string modulo)
        {
            FechaAsistencia = fecha;
            Materia = materia;
            IdCurso = curso;
            Modulo = modulo;

            if (!await ExisteAsistenciaParaFecha(materia, FechaAsistencia, IdCurso))
            {
                await GetAlumnosAsync(materia);
            }
            else
            {
                // Cargar asistencia para la fecha seleccionada
                Alumnos = await GetAsistenciaAsync(materia, fecha, IdCurso);
            }
            
            return Page();
        }

        public IActionResult OnPostAtras(int curso, string modulo)
        {
            IdCurso = curso;
            Modulo = modulo;
            return RedirectToPage("Materia/ListaMaterias");
        }

        public async Task<bool> ExisteAsistenciaParaFecha(int materia, DateTime fecha, int curso)
        {
            // Cargar asistencia para la fecha seleccionada
            Alumnos = await GetAsistenciaAsync(materia, fecha, curso);

            return Alumnos.Count() > 0;
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

        public async Task GetAlumnosAsync(int materia)
        {
            // Traigo todos los usuarios que son alumnos
            //var todos = await GetUsuariosMateriaAsync(materia);
            var todos = await GetIntegrantesCursosAsync(IdCurso);

            foreach (var alumn in todos)
            {
                Asistencia alumno = new Asistencia
                {
                    Alumno = new Usuario
                    {
                        Apellido = alumn.Usuario.Apellido,
                        Nombre = alumn.Usuario.Nombre
                    },
                    Id_Materia = materia,
                    Id_Alumno = alumn.Usuario.Id,
                    Fecha = DateTime.Now
                };

                Alumnos.Add(alumno);
            }
        }

    

        static async Task<List<Asistencia>> GetAsistenciaAsync(int materia, DateTime fecha, int curso)
        {
            List<Asistencia> getalumnos = new List<Asistencia>();

            //string fechaString = fecha.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            //var fecha2 = new DateTime(2024, 9, 24);

            //string queryParam = Uri.EscapeDataString($"x=>x.id_Materia=={materia} && x.Fecha.Value.Date == new DateTime({fecha2.Year}, {fecha2.Month}, {fecha2.Day}).Date");
            string queryParam = Uri.EscapeDataString($"x=>x.id_Materia=={materia} && x.id_curso=={curso}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Asistencia/GetAsistenciasForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string alumnosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnosJson))
                {
                    getalumnos = JsonConvert.DeserializeObject<List<Asistencia>>(alumnosJson);
                }
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}, {responseContent}");
            }

            return getalumnos.Where(a => a.Fecha?.ToShortDateString() == fecha.ToShortDateString()).ToList();
        }

        public async Task<IActionResult> OnPostGuardarAsistencia(List<Asistencia> Alumnos, bool reporte, int materia, DateTime fecha, int curso, string modulo)
        {
            Materia = materia;
            IdCurso = curso;
            Modulo = modulo;

            if (reporte)
            {
                Fecha = fecha;
                return RedirectToPage("ReporteAsistencia");
            }
            else
            {
                //FechaAsistencia = fecha;

                foreach (var alumno in Alumnos)
                {
                    // Convertir el objeto din�mico a JSON
                    var jsonContent = JsonConvert.SerializeObject(alumno);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
                    HttpResponseMessage response;
                    if (alumno.Id > 0)
                    {
                        response = await client.PutAsync("https://localhost:7130/Asistencia/UpdateAsistencia", content);
                    }
                    else
                    {
                        response = await client.PostAsync("https://localhost:7130/Asistencia/CreateAsistencia", content);
                    }

                    // Manejar errores de la respuesta HTTP
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("asistencia", alumno.Id > 0
                            ? "Hubo un error inesperado al actualizar la Asistencia: " + errorResponse
                            : "Hubo un error inesperado al crear la Asistencia: " + errorResponse);

                        await OnGetAsync();
                        return Page();
                    }
                }

                TempData["SuccessMessage"] = "La asistencia se guard� correctamente.";
                return RedirectToPage("Asistencia");
            }
        }
    }

}
