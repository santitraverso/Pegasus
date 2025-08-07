using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Dynamic;
using System;
using Microsoft.Extensions.Options;

namespace PegasusWeb.Pages
{
    public class CreateCursoModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public CreateCursoModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        [BindProperty]
        public Curso? Curso { get; set; }

        [TempData]
        public int IdCurso { get; set; }

        public List<IntegrantesCursos> Alumnos { get; set; } = new List<IntegrantesCursos>();
        public List<CursoMateria> Materias { get; set; } = new List<CursoMateria>();

        [BindProperty]
        public List<int> SelectedAlumnosIds { get; set; } = new List<int>(); // IDs de los alumnos seleccionados en el formulario

        [TempData]
        public int IdPerfil { get; set; }

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

            IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;

            if (IdCurso > 0)
            {
                Curso = await GetCursoAsync(IdCurso);

                if(Curso != null)
                {
                    Alumnos = await GetIntegrantesCursosAsync(IdCurso);

                    Materias = await GetMateriasCursoAsync(IdCurso);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                Curso = new Curso { Id = 0 };
            }

            return Page();
        }

        public async Task<Curso> GetCursoAsync(int curso)
        {
            Curso getCurso = new Curso();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Curso/GetById?id={curso}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string cursoJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursoJson))
                {
                    getCurso = JsonConvert.DeserializeObject<Curso>(cursoJson);
                }
            }

            return getCurso;
        }

        private async Task<List<CursoMateria>> GetMateriasCursoAsync(int curso)
        {
            List<CursoMateria> getMaterias = new List<CursoMateria>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso}");
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
                    getMaterias = JsonConvert.DeserializeObject<List<CursoMateria>>(materiasJson);
                }
            }

            return getMaterias;
        }

        public IActionResult OnPostAgregarAlumno(int curso)
        {
            IdCurso = curso;
            return RedirectToPage("IntegrantesCursos");
        }

        public IActionResult OnPostAgregarMateria(int curso)
        {
            IdCurso = curso;
            return RedirectToPage("MateriasCurso");
        }

        async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso}");
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

        public async Task<IActionResult> OnPostAsync(bool atras, byte grado, string nombre, char division, string turno, int id)
        {
            if (atras)
            {
                return RedirectToPage("Curso");
            }
            else
            {
                // Validaciones de entrada
                if (grado < 1)
                    ModelState.AddModelError("grado", "El campo Grado es requerido");

                if (string.IsNullOrEmpty(nombre))
                    ModelState.AddModelError("nombre", "El campo Nombre es requerido");

                if (division == '\0')
                    ModelState.AddModelError("division", "El campo Division es requerido");

                if (string.IsNullOrEmpty(turno))
                    ModelState.AddModelError("turno", "El campo Turno es requerido");

                if (!ModelState.IsValid)
                {
                    await OnGetAsync();
                    return Page();
                }

                dynamic cursoData = new ExpandoObject();
                cursoData.Nombre_Curso = nombre;
                cursoData.Grado = grado;
                cursoData.Division = division;
                cursoData.Turno = turno;

                if (id > 0)
                {
                    cursoData.Id = id;
                }

                // Convertir el objeto dinámico a JSON
                var jsonContent = JsonConvert.SerializeObject(cursoData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
                HttpResponseMessage response;
                if (id > 0)
                {
                    var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/Curso/UpdateCurso");

                    request.Content = content;

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
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/Curso/CreateCurso");

                    request.Content = content;

                    // Añadir el token JWT al encabezado
                    string token = HttpContext.Session.GetString("JwtToken");
                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Add("Authorization", $"Bearer {token}");
                    }

                    response = await _client.SendAsync(request);
                }

                // Manejar errores de la respuesta HTTP
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("curso", id > 0
                        ? "Hubo un error inesperado al actualizar el Curso: " + errorResponse
                        : "Hubo un error inesperado al crear el Curso: " + errorResponse);

                    await OnGetAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = "El curso se guardó correctamente.";
                return RedirectToPage("Curso");
            }
        }
    }
}
