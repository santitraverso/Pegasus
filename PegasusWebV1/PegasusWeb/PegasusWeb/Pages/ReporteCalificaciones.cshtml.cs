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
    public class ReporteCalificacionesModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public ReporteCalificacionesModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<IntegrantesMaterias> Alumnos { get; set; } = new List<IntegrantesMaterias>();

        [TempData]
        public int Materia { get; set; }
        [TempData]
        public int IdCurso { get; set; }
        [BindProperty]
        public string? Nombre { get; set; }
        [TempData]
        public string? Modulo { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar sesión válida
            string token = HttpContext.Session.GetString("JwtToken") ?? "";
            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;

            if (string.IsNullOrEmpty(token) || idUsuario <= 0 || idPerfil <= 0)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Index");
            }

            var alumnos = new List<IntegrantesCursos>();

            if (idPerfil == (int)TipoPerfil.Alumno)
            {
                alumnos = await GetIntegrantesCursosAsync(IdCurso, idUsuario);
            }
            else if (idPerfil == (int)TipoPerfil.Padre)
            {
                alumnos = await GetIntegrantesCursosAsync(IdCurso, HttpContext.Session.GetInt32("IdHijo") ?? idUsuario);
            }
            else
            {
                alumnos = await GetIntegrantesCursosAsync(IdCurso);
            }

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

            return Page();
        }


        public IActionResult OnPostAtras(int materia, int curso, string modulo)
        {
            Materia = materia;
            IdCurso = curso;
            Modulo = modulo;
            return RedirectToPage("Calificacion");
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

        public async Task<List<Calificaciones>> GetCalificacionesAsync(int materia, int curso, int usuario)
        {
            List<Calificaciones> getCalificaciones = new List<Calificaciones>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_materia=={materia} && x.id_curso=={curso} && x.id_alumno=={usuario}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Calificaciones/GetCalificacionesForCombo?query={queryParam}");

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
                    getCalificaciones = JsonConvert.DeserializeObject<List<Calificaciones>>(alumnosJson);
                }
            }

            return getCalificaciones;
        }

        async Task<Entities.Materia> GetNombreMateriaAsync(int materia)
        {
            Entities.Materia getMateria = new Entities.Materia();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Materia/GetById?id={materia}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

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
