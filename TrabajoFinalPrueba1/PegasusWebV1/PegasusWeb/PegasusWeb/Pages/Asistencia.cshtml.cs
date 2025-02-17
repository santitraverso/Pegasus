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

        [TempData]
        public int IdPerfil { get; set; }

        [TempData]
        public int IdUsuario { get; set; }

        public async Task OnGetAsync()
        {
            if (Fecha != DateTime.MinValue)
                FechaAsistencia = Fecha;

            IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;
            IdUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            // Verificar si hay asistencia para el día actual
            if (!await ExisteAsistenciaParaFecha(Materia, FechaAsistencia, IdCurso))
            {
                // Si no hay asistencia, obtener todos los alumnos
                await GetAlumnosAsync(Materia);
            }
            else
            {
                // Si ya hay asistencia, cargar los alumnos con su asistencia
                var alumnos = await GetAsistenciaAsync(Materia, FechaAsistencia, IdCurso);

                if (IdPerfil == 2)
                    Alumnos = alumnos.Where(alu => alu.Id_Alumno == IdUsuario).ToList();
                else if (IdPerfil == 4)
                    Alumnos = alumnos.Where(alu => alu.Id_Alumno == HttpContext.Session.GetInt32("IdHijo")).ToList();
                else
                    Alumnos = alumnos;
            }
        }

        public async Task<IActionResult> OnPost(DateTime fecha, int materia, int curso, string modulo, int perfil, int usuario)
        {
            FechaAsistencia = fecha;
            Materia = materia;
            IdCurso = curso;
            Modulo = modulo;
            IdPerfil = perfil;
            IdUsuario = usuario;

            if (!await ExisteAsistenciaParaFecha(materia, FechaAsistencia, IdCurso))
            {
                await GetAlumnosAsync(materia);
            }
            else
            {
                // Si ya hay asistencia, cargar los alumnos con su asistencia
                var alumnos = await GetAsistenciaAsync(materia, fecha, IdCurso);

                if (IdPerfil == 2)
                    Alumnos = alumnos.Where(alu => alu.Id_Alumno == IdUsuario).ToList();
                else if (IdPerfil == 4)
                    Alumnos = alumnos.Where(alu => alu.Id_Alumno == HttpContext.Session.GetInt32("IdHijo")).ToList();
                else
                    Alumnos = alumnos;
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
            var alumnos = await GetAsistenciaAsync(materia, fecha, curso);

            if (IdPerfil == 2)
                return alumnos.Any(alu => alu.Id_Alumno == IdUsuario);
            else if(IdPerfil == 4)
                return alumnos.Any(alu => alu.Id_Alumno == HttpContext.Session.GetInt32("IdHijo"));
            else
                return alumnos.Count() > 0;
        }


        public static async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso, int usuario = 0)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();
            string queryParam;

            if (usuario != 0)
                queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso} && x.id_usuario=={usuario}");
            else
                queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso}");

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
            var todos = new List<IntegrantesCursos>();

            if (IdPerfil == 2)
            {
                todos = await GetIntegrantesCursosAsync(IdCurso, IdUsuario);
            }
            else if (IdPerfil == 4)
            {
                todos = await GetIntegrantesCursosAsync(IdCurso, HttpContext.Session.GetInt32("IdHijo") ?? IdUsuario);
            }
            else
            {
                todos = await GetIntegrantesCursosAsync(IdCurso);
            }

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
                    // Convertir el objeto dinámico a JSON
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

                TempData["SuccessMessage"] = "La asistencia se guardó correctamente.";
                return RedirectToPage("Asistencia");
            }
        }
    }

}
