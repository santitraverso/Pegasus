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
        public int IdMateria { get; set; }

        [TempData]
        public DateTime Fecha { get; set; }

        public async Task OnGetAsync()
        {
            // Verificar si hay asistencia para el día actual
            if (!await ExisteAsistenciaParaFecha(1, FechaAsistencia))
            {
                // Si no hay asistencia, obtener todos los alumnos
                await GetAlumnosAsync(1);
            }
            else
            {
                // Si ya hay asistencia, cargar los alumnos con su asistencia
                Alumnos = await GetAsistenciaAsync(1, FechaAsistencia);
            }
        }

        public async Task<IActionResult> OnPost(DateTime fecha)
        {
            FechaAsistencia = fecha;

            if (fecha.ToShortDateString() == DateTime.Now.ToShortDateString())
            {
                await GetAlumnosAsync(1);
            }
            else
            {
                // Cargar asistencia para la fecha seleccionada
                Alumnos = await GetAsistenciaAsync(1, fecha);
            }
            
            return Page();
        }

        public async Task<bool> ExisteAsistenciaParaFecha(int materia, DateTime fecha)
        {
            // Cargar asistencia para la fecha seleccionada
            Alumnos = await GetAsistenciaAsync(1, fecha);

            return Alumnos.Count() > 0;
        }

        public static async Task<List<Usuario>> GetUsuariosAlumnosAsync()
        {
            List<Usuario> getalumnos = new List<Usuario>();

            string queryParam = Uri.EscapeDataString("x=>x.perfil == 2 && x.activo == true");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Usuario/GetUsuariosForCombo?query={queryParam}");
            if (response.IsSuccessStatusCode)
            {
                string alumnosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnosJson))
                {
                    getalumnos = JsonConvert.DeserializeObject<List<Usuario>>(alumnosJson);
                }
            }

            return getalumnos;
        }

        static async Task<List<IntegrantesMaterias>> GetIntegrantesMateriasAsync(int materia)
        {
            List<IntegrantesMaterias> getalumnos = new List<IntegrantesMaterias>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            string queryParam = Uri.EscapeDataString($"x=>x.id_materia=={materia}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/IntegrantesMaterias/GetIntegrantesMateriasForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string alumnosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnosJson))
                {
                    getalumnos = JsonConvert.DeserializeObject<List<IntegrantesMaterias>>(alumnosJson);
                }
            }

            return getalumnos;
        }

        public async Task GetAlumnosAsync(int materia)
        {
            // Traigo todos los usuarios que son alumnos
            var todos = await GetUsuariosMateriaAsync(materia);

            foreach (var alumn in todos)
            {
                Asistencia alumno = new Asistencia
                {
                    Alumno = new Usuario
                    {
                        Apellido = alumn.Apellido,
                        Nombre = alumn.Nombre
                    },
                    Id_Materia = materia,
                    Id_Alumno = alumn.Id,
                    Fecha = DateTime.Now
                };

                Alumnos.Add(alumno);
            }
        }

        private static async Task<IEnumerable<Usuario>> GetUsuariosMateriaAsync(int materia)
        {
            var usuarios = await GetUsuariosAlumnosAsync();

            var integrantesMateria = await GetIntegrantesMateriasAsync(materia);

            return usuarios.Where(u => integrantesMateria.Exists(i => i.Id_Usuario == u.Id)).ToList();
        }

        static async Task<List<Asistencia>> GetAsistenciaAsync(int materia, DateTime fecha)
        {
            List<Asistencia> getalumnos = new List<Asistencia>();

            //string fechaString = fecha.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            //var fecha2 = new DateTime(2024, 9, 24);

            //string queryParam = Uri.EscapeDataString($"x=>x.id_Materia=={materia} && x.Fecha.Value.Date == new DateTime({fecha2.Year}, {fecha2.Month}, {fecha2.Day}).Date");
            string queryParam = Uri.EscapeDataString($"x=>x.id_Materia=={materia}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Asistencia/GetAsistenciasForCombo?query={queryParam}");

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

        public async Task<IActionResult> OnPostGuardarAsistencia(List<Asistencia> Alumnos, bool reporte, int materia, DateTime fecha)
        {
            if (reporte)
            {
                IdMateria = materia;
                Fecha = fecha;
                return RedirectToPage("ReporteAsistencia");
            }
            else
            {
                //FechaAsistencia = fecha;

                foreach (var alumno in Alumnos)
                {
                    // Convertir el objeto dinámico a JSON
                    var jsonContent = JsonConvert.SerializeObject(alumno);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
                    HttpResponseMessage response;
                    if (alumno.Id > 0)
                    {
                        response = await client.PutAsync("http://localhost:7130/Asistencia/UpdateAsistencia", content);
                    }
                    else
                    {
                        response = await client.PostAsync("http://localhost:7130/Asistencia/CreateAsistencia", content);
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

                return RedirectToPage("Asistencia");
            }
        }
    }

}
