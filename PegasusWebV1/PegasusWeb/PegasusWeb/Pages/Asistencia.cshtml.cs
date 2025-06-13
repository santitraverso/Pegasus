using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Dynamic;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class AsistenciaModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public AsistenciaModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<Asistencia> Alumnos { get; set; } = new List<Asistencia>();
        public DateTime FechaAsistencia { get; set; } = DateTime.Now;

        [TempData]
        public int Materia { get; set; }

        [TempData]
        public DateTime Fecha { get; set; }

        [TempData]
        public int IdCurso { get; set; }

        [TempData]
        public string? Modulo { get; set; }

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

                if (IdPerfil == (int)TipoPerfil.Alumno)
                    Alumnos = alumnos.Where(alu => alu.Id_Alumno == IdUsuario).ToList();
                else if (IdPerfil == (int)TipoPerfil.Padre)
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

                if (IdPerfil == (int)TipoPerfil.Alumno)
                    Alumnos = alumnos.Where(alu => alu.Id_Alumno == IdUsuario).ToList();
                else if (IdPerfil == (int)TipoPerfil.Padre)
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

            if (IdPerfil == (int)TipoPerfil.Alumno)
                return alumnos.Any(alu => alu.Id_Alumno == IdUsuario);
            else if(IdPerfil == (int)TipoPerfil.Padre)
                return alumnos.Any(alu => alu.Id_Alumno == HttpContext.Session.GetInt32("IdHijo"));
            else
                return alumnos.Count() > 0;
        }


        public async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso, int usuario = 0)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();
            string queryParam;

            if (usuario != 0)
                queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso} && x.id_usuario=={usuario}");
            else
                queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso}");

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/IntegrantesCursos/GetIntegrantesCursosForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

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
            var todos = new List<IntegrantesCursos>();

            if (IdPerfil == (int)TipoPerfil.Alumno)
            {
                todos = await GetIntegrantesCursosAsync(IdCurso, IdUsuario);
            }
            else if (IdPerfil == (int)TipoPerfil.Padre)
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

    

        async Task<List<Asistencia>> GetAsistenciaAsync(int materia, DateTime fecha, int curso)
        {
            List<Asistencia> getalumnos = new List<Asistencia>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_Materia=={materia} && x.id_curso=={curso}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Asistencia/GetAsistenciasForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

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
                // Separar alumnos en dos listas: para crear y para actualizar
                var alumnosParaCrear = Alumnos.Where(a => a.Id <= 0).ToList();
                var alumnosParaActualizar = Alumnos.Where(a => a.Id > 0).ToList();

                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                if (alumnosParaCrear.Any())
                {
                    var jsonContent = JsonConvert.SerializeObject(alumnosParaCrear);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/Asistencia/CreateAllAsistencia");
                    request.Content = content;

                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Add("Authorization", $"Bearer {token}");
                    }

                    var response = await _client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("asistencia", "Hubo un error al crear las asistencias: " + errorResponse);
                        await OnGetAsync();
                        return Page();
                    }
                }

                if (alumnosParaActualizar.Any())
                {
                    var jsonContent = JsonConvert.SerializeObject(alumnosParaActualizar);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/Asistencia/UpdateAllAsistencia");
                    request.Content = content;

                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Add("Authorization", $"Bearer {token}");
                    }

                    var response = await _client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("asistencia", "Hubo un error al actualizar las asistencias: " + errorResponse);
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
