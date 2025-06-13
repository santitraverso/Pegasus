using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class DesempenioModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public DesempenioModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<DesempenioAlumnos> Alumnos { get; set; } = new List<DesempenioAlumnos>();

        [TempData]
        public int IdDesempenio { get; set; }

        [TempData]
        public int IdCurso { get; set; }

        [TempData]
        public string? Modulo { get; set; }

        [BindProperty]
        public List<int> SelectedAlumnosIds { get; set; } = new List<int>();

        [TempData]
        public bool Ver { get; set; }
        [TempData]
        public int IdAlumno { get; set; }
        [TempData]
        public int IdPerfil { get; set; }


        public async Task OnGetAsync()
        {
            var integrantes = new List<IntegrantesCursos>();
            IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            if (IdPerfil == (int)TipoPerfil.Alumno)
            {
                integrantes = await GetIntegrantesCursosAsync(IdCurso, idUsuario);
            }
            else if (IdPerfil == (int)TipoPerfil.Padre)
            {
                integrantes = await GetIntegrantesCursosAsync(IdCurso, HttpContext.Session.GetInt32("IdHijo") ?? idUsuario);
            }
            else
            {
                integrantes = await GetIntegrantesCursosAsync(IdCurso);
            }

            Alumnos = integrantes.Select(alumn => new DesempenioAlumnos
            {
                Alumno = alumn.Usuario,
                Id_Alumno = alumn.Id_Usuario
            }).ToList();

            var desempenios = await GetDesempenoAlumnosAsync(IdCurso);

            var desempenosIds = desempenios.Select(d => d.Id_Alumno).ToHashSet();

            SelectedAlumnosIds = integrantes
                .Where(alumn => desempenosIds.Contains(alumn.Id_Usuario))
                .Select(alumn => (int)alumn.Id_Usuario)
                .ToList();

            foreach (var alu in Alumnos)
            {
                var desempenio = desempenios.FirstOrDefault(d => d.Id_Alumno == alu.Id_Alumno);
                if (desempenio != null)
                {
                    alu.Id = desempenio.Id;
                }
            }
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

        public IActionResult OnPostAtras(int curso, string modulo)
        {
            IdCurso = curso;
            Modulo = modulo;
            return RedirectToPage("ListaCursos");
        }

        public async Task<IActionResult> OnPost(int desempenio, bool ver, int curso, string modulo, int alumno)
        {
            IdDesempenio = desempenio;
            Ver = ver;
            IdCurso = curso;
            Modulo = modulo;
            IdAlumno = alumno;

            return RedirectToPage("CreateDesempenio");
        }

        async Task<List<DesempenioAlumnos>> GetDesempenoAlumnosAsync(int curso)
        {
            List<DesempenioAlumnos> getusuarios = new List<DesempenioAlumnos>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso == {curso}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/DesempenioAlumnos/GetDesempenioAlumnossForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string usuariosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(usuariosJson))
                {
                    getusuarios = JsonConvert.DeserializeObject<List<DesempenioAlumnos>>(usuariosJson);
                }
            }

            return getusuarios;
        }
    }
}
