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
    public class ReporteAsistenciaModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public ReporteAsistenciaModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }
        public List<Asistencia> Alumnos { get; set; } = new List<Asistencia>();

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

            IdPerfil = idPerfil; 
            IdUsuario = idUsuario;

            var alumnos = await GetAsistenciaAsync(Materia, Fecha, IdCurso);

            if (IdPerfil == (int)TipoPerfil.Alumno)
                Alumnos = alumnos.Where(alu => alu.Id_Alumno == IdUsuario).ToList();
            else if (IdPerfil == (int)TipoPerfil.Padre)
                Alumnos = alumnos.Where(alu => alu.Id_Alumno == HttpContext.Session.GetInt32("IdHijo")).ToList();
            else
                Alumnos = alumnos;

            return Page();
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

        public IActionResult OnPostAtras(int materia, DateTime fecha, int curso, string modulo)
        {
            Materia = materia;
            Fecha = fecha;
            Modulo = modulo;
            IdCurso = curso;
            return RedirectToPage("Asistencia");
        }
    }

}
