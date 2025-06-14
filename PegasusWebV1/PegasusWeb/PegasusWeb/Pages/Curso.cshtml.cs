using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class CursoModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public CursoModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<Curso>? Cursos { get; set; }

        [TempData]
        public int IdCurso { get; set; }
        [TempData]
        public int IdPerfil { get; set; }
        [TempData]
        public int IdUsuario { get; set; }

        public async Task OnGetAsync()
        {
            IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;
            IdUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            if (IdPerfil == (int)TipoPerfil.Docente)
            {
                // Obtener las materias asignadas al docente
                var cursosDocente = await GetCursosDocenteAsync(IdUsuario);

                // Extraer los IDs de cursos únicos de las materias del docente
                var idsCursosDocente = cursosDocente
                    .Where(dm => dm.Id_Curso.HasValue)
                    .Select(dm => dm.Id_Curso.Value)
                    .Distinct()
                    .ToList();

                if (idsCursosDocente.Any())
                {
                    // Obtener todos los cursos del docente
                    var todosCursos = await GetCursosAsync();
                    Cursos = todosCursos.Where(c => idsCursosDocente.Contains((int)c.Id)).ToList();
                }
                else
                {
                    // Si el docente no tiene cursos asignados, mostrar lista vacía
                    Cursos = new List<Curso>();
                }
            }
            else
                Cursos = await GetCursosAsync();
        }

        private async Task<bool> TieneIntegrantesCurso(int curso)
        {
            List<IntegrantesCursos> getintegrantes = new List<IntegrantesCursos>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso == {curso}");
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
                string integrantesJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(integrantesJson))
                {
                    getintegrantes = JsonConvert.DeserializeObject<List<IntegrantesCursos>>(integrantesJson);
                }
            }

            return getintegrantes.Count > 0;
        }

        private async Task<bool> TieneMateriasCurso(int curso)
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

            return getmaterias.Count > 0;
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


        async Task<List<Curso>> GetCursosAsync()
        {
            List<Curso> getcursos = new List<Curso>();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Curso/GetCursosForCombo");

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
                    getcursos = JsonConvert.DeserializeObject<List<Curso>>(cursosJson);
                }
            }

            return getcursos;
        }


        public async Task<IActionResult> OnPostAsync(int curso, bool editar)
        {
            IdCurso = curso;

            if (editar)
            {
                return RedirectToPage("CreateCurso");
            }
            else
            {
                try
                {
                    // Verificar si tiene integrantes o materias
                    var tieneIntegrantes = await TieneIntegrantesCurso(curso);
                    var tieneMaterias = await TieneMateriasCurso(curso);

                    // Eliminar todas las relaciones y luego el curso
                    bool eliminacionExitosa = await EliminarCursoConRelacionesAsync(curso, tieneIntegrantes, tieneMaterias);

                    if (eliminacionExitosa)
                    {
                        TempData["SuccessMessage"] = "El curso y todas sus relaciones se eliminaron correctamente.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Ocurrió un error al eliminar el curso o sus relaciones.";
                    }

                    Cursos = await GetCursosAsync();
                    return RedirectToPage("Curso");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("curso", $"Error al eliminar el curso: {ex.Message}");
                    Cursos = await GetCursosAsync();
                    return Page();
                }
            }
        }

        public async Task<bool> EliminarCursoConRelacionesAsync(int cursoId, bool tieneIntegrantes, bool tieneMaterias)
        {
            try
            {
                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                //Eliminar integrantes del curso si existen
                if (tieneIntegrantes)
                {
                    bool integrantesEliminados = await EliminarIntegrantesCursoAsync(cursoId, token);
                    if (!integrantesEliminados)
                    {
                        ModelState.AddModelError("curso", "No se pudieron eliminar los integrantes del curso.");
                        return false;
                    }
                }

                //Eliminar materias del curso si existen
                if (tieneMaterias)
                {
                    bool materiasEliminadas = await EliminarMateriasCursoAsync(cursoId, token);
                    if (!materiasEliminadas)
                    {
                        ModelState.AddModelError("curso", "No se pudieron eliminar las materias del curso.");
                        return false;
                    }
                }

                // Eliminar el curso
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/Curso/DeleteCurso/{cursoId}");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage response = await _client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("curso", $"Error al eliminar el curso: {errorResponse}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("curso", $"Error en el proceso de eliminación: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> EliminarIntegrantesCursoAsync(int cursoId, string token)
        {
            try
            {
                //Obtener todos los integrantes del curso
                List<IntegrantesCursos> integrantes = await ObtenerIntegrantesCursoAsync(cursoId, token);

                if (integrantes.Count == 0)
                {
                    return true; // No hay integrantes que eliminar
                }

                var integrantesSimplificados = integrantes.Select(i => new { Id = i.Id }).ToList();
                var jsonContent = JsonConvert.SerializeObject(integrantesSimplificados);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/IntegrantesCursos/DeleteAllIntegrantesCursos");
                request.Content = content;

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage response = await _client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("curso", $"Error al eliminar integrantes: {errorResponse}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("curso", $"Error al eliminar integrantes: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> EliminarMateriasCursoAsync(int cursoId, string token)
        {
            try
            {
                //Obtener todas las materias del curso
                List<CursoMateria> materias = await ObtenerMateriasCursoAsync(cursoId, token);

                if (materias.Count == 0)
                {
                    return true; // No hay materias que eliminar
                }

                var materiasSimplificadas = materias.Select(m => new { Id = m.Id }).ToList();
                var jsonContent = JsonConvert.SerializeObject(materiasSimplificadas);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/CursoMateria/DeleteAllCursoMateria");
                request.Content = content;

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage response = await _client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("curso", $"Error al eliminar materias: {errorResponse}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("curso", $"Error al eliminar materias: {ex.Message}");
                return false;
            }
        }

        private async Task<List<IntegrantesCursos>> ObtenerIntegrantesCursoAsync(int cursoId, string token)
        {
            List<IntegrantesCursos> integrantes = new List<IntegrantesCursos>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso == {cursoId}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/IntegrantesCursos/GetIntegrantesCursosForCombo?query={queryParam}");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string integrantesJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(integrantesJson))
                {
                    integrantes = JsonConvert.DeserializeObject<List<IntegrantesCursos>>(integrantesJson);
                }
            }

            return integrantes;
        }

        private async Task<List<CursoMateria>> ObtenerMateriasCursoAsync(int cursoId, string token)
        {
            List<CursoMateria> materias = new List<CursoMateria>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso == {cursoId}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/CursoMateria/GetCursoMateriaForCombo?query={queryParam}");

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
                    materias = JsonConvert.DeserializeObject<List<CursoMateria>>(materiasJson);
                }
            }

            return materias;
        }
    }

}
