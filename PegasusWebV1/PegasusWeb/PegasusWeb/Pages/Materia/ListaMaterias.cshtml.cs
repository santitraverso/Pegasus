using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages.Materia
{
    public class ListaMateriasModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public ListaMateriasModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }
        public List<CursoMateria> Materias { get; set; } = new List<CursoMateria> { };

        [TempData]
        public int Materia { get; set; }

        [TempData]
        public int IdCurso { get; set; }

        [TempData]
        public string? Modulo { get; set; }

        [TempData]
        public int IdUsuario { get; set; }

        [TempData]
        public int IdPerfil { get; set; }

        public async Task OnGetAsync()
        {

            IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;
            IdUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            if(IdPerfil == (int)TipoPerfil.Docente)
            {
                var materiasDocente = await GetMateriasDocenteAsync(IdUsuario, IdCurso);
                Materias = materiasDocente
                    .GroupBy(materia => materia.Id_Materia)
                    .Select(group => group.First())
                    .Select(materia => new CursoMateria
                    {
                        Curso = materia.Curso,
                        Id_Curso = materia.Id_Curso,
                        Materia = materia.Materia,
                        Id_Materia = materia.Id_Materia,
                    })
                    .ToList();
            }
            else
            {
                Materias = await GetMateriasAsync(IdCurso);
            }
        }

        async Task<List<CursoMateria>> GetMateriasAsync(int curso)
        {
            List<CursoMateria> getmaterias = new List<CursoMateria>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso == {curso}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/CursoMateria/GetCursoMateriaForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string materiasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(materiasJson))
                {
                    getmaterias = JsonConvert.DeserializeObject<List<CursoMateria>>(materiasJson);
                }
            }

            return getmaterias;
        }

        public async Task<List<DocenteMateria>> GetMateriasDocenteAsync(int docente, int curso)
        {
            List<DocenteMateria> getcursos = new List<DocenteMateria>();
            string queryParam = Uri.EscapeDataString($"x=>x.id_docente=={docente} && x.id_curso=={curso}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/DocenteMateria/GetDocenteMateriaForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);


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

        public async Task<IActionResult> OnPostAsync(int materia, string modulo, int curso)
        {
            Materia = materia;
            Modulo = modulo;
            IdCurso = curso;

            switch (modulo)
            {
                case "Calificacion":
                    return RedirectToPage("../Calificacion");
                case "Asistencia":
                    return RedirectToPage("../Asistencia");
                default: 
                    return RedirectToPage("../Index");
            }
        }
    }
}
