using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ListaCursosModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public ListaCursosModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }
        public List<IntegrantesCursos>? Cursos { get; set; }

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

            switch (IdPerfil)
            {
                //Alumno
                case (int)TipoPerfil.Alumno:
                    Cursos = await GetCursosAsync(IdUsuario);
                    break;
                //Docente
                case (int)TipoPerfil.Docente:
                    var cursosDocente = await GetCursosDocenteAsync(IdUsuario);
                    Cursos = cursosDocente
                        .GroupBy(curso => curso.Id_Curso)
                        .Select(group => group.First())
                        .Select(curso => new IntegrantesCursos
                        {
                            Usuario = curso.Docente,
                            Id_Usuario = curso.Id_Docente,
                            Curso = curso.Curso,
                            Id_Curso = curso.Id_Curso,
                        })
                        .ToList();
                    break;
                //Padre
                case (int)TipoPerfil.Padre:
                    Cursos = await GetCursosAsync(HttpContext.Session.GetInt32("IdHijo") ?? IdUsuario);
                    break;

                default:
                    var cursos = await GetCursosAsync();
                    Cursos = cursos
                        .GroupBy(ic => ic.Id_Curso)
                        .Select(g => g.First())
                        .OrderBy(c => c.Id_Curso)
                        .ToList();
                    break;
            }
        }

        async Task<List<IntegrantesCursos>> GetCursosAsync(int usuario = 0)
        {
            List<IntegrantesCursos> getcursos = new List<IntegrantesCursos>();
            HttpResponseMessage response;

            if (usuario > 0)
            {
                string queryParam = Uri.EscapeDataString($"x=>x.id_usuario=={usuario}");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/IntegrantesCursos/GetIntegrantesCursosForCombo?query={queryParam}");

                // Añadir el token JWT al encabezado
                string token = HttpContext.Session.GetString("JwtToken");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                response = await _client.SendAsync(request);
            }
            else
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/IntegrantesCursos/GetIntegrantesCursosForCombo");

                // Añadir el token JWT al encabezado
                string token = HttpContext.Session.GetString("JwtToken");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                response = await _client.SendAsync(request);
            }
           
            if (response.IsSuccessStatusCode)
            {
                string cursosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursosJson))
                {
                    getcursos = JsonConvert.DeserializeObject<List<IntegrantesCursos>>(cursosJson);
                }
            }

            return getcursos;
        }

        public async Task<List<DocenteMateria>> GetCursosDocenteAsync(int docente)
        {
            List<DocenteMateria> getcursos = new List<DocenteMateria>();
            string queryParam = Uri.EscapeDataString($"x=>x.id_docente=={docente}");
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

        public IActionResult OnPost(int curso, string modulo, int usuario, int perfil)
        {
            IdCurso = curso;
            Modulo = modulo;
            IdUsuario = usuario;
            IdPerfil = perfil;

            switch (modulo)
            {
                case "Cuaderno":
                    return RedirectToPage("Cuaderno");
                case "Desempenio":
                    return RedirectToPage("Desempenio");
                default:
                    return RedirectToPage("Materia/ListaMaterias");
            }

        }
    }
}
