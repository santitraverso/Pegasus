using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class MateriaModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public MateriaModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<Entities.Materia>? Materias { get; set; }

        [TempData]
        public int IdMateria { get; set; }
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

            try
            {
                switch (IdPerfil)
                {
                    case (int)TipoPerfil.Alumno:
                        Materias = await GetMateriasAlumnoAsync(IdUsuario);
                        break;

                    case (int)TipoPerfil.Docente:
                        Materias = await GetMateriasDocenteAsync(IdUsuario);
                        break;

                    case (int)TipoPerfil.Padre:
                        Materias = await GetMateriasAlumnoAsync(HttpContext.Session.GetInt32("IdHijo") ?? IdUsuario);
                        break;

                    case (int)TipoPerfil.Admin:
                    case (int)TipoPerfil.Preceptor:
                    default:
                        Materias = await GetMateriasAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al cargar las materias");
                Materias = new List<Entities.Materia>();
            }

            return Page();
        }

        private async Task<List<Entities.Materia>> GetMateriasAlumnoAsync(int alumnoId)
        {
            // Obtener cursos del alumno
            var integrantesCursos = await GetIntegrantesCursosAsync(alumnoId);

            var idsCursos = integrantesCursos
                .Where(ic => ic.Id_Curso.HasValue)
                .Select(ic => ic.Id_Curso.Value)
                .Distinct()
                .ToList();

            if (!idsCursos.Any())
            {
                return new List<Entities.Materia>();
            }

            var idsMaterias = await GetIdsMateriasPorCursosAsync(idsCursos);

            if (!idsMaterias.Any())
            {
                return new List<Entities.Materia>();
            }

            // Filtrar materias
            var todasMaterias = await GetMateriasAsync();
            return todasMaterias.Where(m => idsMaterias.Contains((int)m.Id)).ToList();
        }

        private async Task<List<Entities.Materia>> GetMateriasDocenteAsync(int docenteId)
        {
            // Obtener materias que dicta el docente
            var docenteMaterias = await GetDocenteMateriasAsync(docenteId);

            var idsMaterias = docenteMaterias
                .Where(dm => dm.Id_Materia.HasValue)
                .Select(dm => dm.Id_Materia.Value)
                .Distinct()
                .ToList();

            if (!idsMaterias.Any())
            {
                return new List<Entities.Materia>();
            }

            var todasMaterias = await GetMateriasAsync();
            return todasMaterias.Where(m => idsMaterias.Contains((int)m.Id)).ToList();
        }


        private async Task<List<int>> GetIdsMateriasPorCursosAsync(List<int> idsCursos)
        {
            var idsMaterias = new List<int>();

            string cursosIds = string.Join(",", idsCursos);
            string queryParam = Uri.EscapeDataString($"x => {cursosIds}.Contains(x.id_curso.Value)");

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_apiBaseUrl}/CursoMateria/GetCursoMateriaForCombo?query={queryParam}");

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
                    var cursosMaterias = JsonConvert.DeserializeObject<List<CursoMateria>>(materiasJson);
                    idsMaterias = cursosMaterias
                        .Where(cm => cm.Id_Materia.HasValue)
                        .Select(cm => cm.Id_Materia.Value)
                        .Distinct()
                        .ToList();
                }
            }

            return idsMaterias;
        }

        private async Task<List<DocenteMateria>> GetDocenteMateriasAsync(int docenteId)
        {
            List<DocenteMateria> docenteMaterias = new List<DocenteMateria>();
            string queryParam = Uri.EscapeDataString($"x=>x.id_docente=={docenteId}");

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_apiBaseUrl}/DocenteMateria/GetDocenteMateriaForCombo?query={queryParam}");

            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string docenteMateriasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(docenteMateriasJson))
                {
                    docenteMaterias = JsonConvert.DeserializeObject<List<DocenteMateria>>(docenteMateriasJson);
                }
            }

            return docenteMaterias;
        }

        public async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int usuario)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();
            string queryParam = Uri.EscapeDataString($"x=>x.id_usuario=={usuario}");

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


        private async Task<bool> TieneAsistenciasMateria(int materia)
        {
            List<Asistencia> getasistencias = new List<Asistencia>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_materia == {materia}");
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
                string asistenciasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(asistenciasJson))
                {
                    getasistencias = JsonConvert.DeserializeObject<List<Asistencia>>(asistenciasJson);
                }
            }

            return getasistencias.Count > 0;
        }

        public async Task<List<Curso>> GetCursosMateriaAsync(int materia)
        {
            List<Curso> getCursos = new List<Curso>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_materia == {materia}");
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
                string cursosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursosJson))
                {
                    getCursos = JsonConvert.DeserializeObject<List<Curso>>(cursosJson);
                }
            }

            return getCursos;
        }

        async Task<List<Entities.Materia>> GetMateriasAsync()
        {
            List <Entities.Materia> getMaterias = new List<Entities.Materia>();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Materia/GetMateriasForCombo");

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
                    getMaterias = JsonConvert.DeserializeObject<List<Entities.Materia>>(materiasJson);
                }
            }

            return getMaterias;
        }

        public async Task EliminarMateriaAsync(int materia)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/Materia/DeleteMateria/{materia}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                this.ModelState.AddModelError("materia", "Hubo un error inesperado al borrar la Materia");
            }
        }

        public async Task<IActionResult> OnPostAsync(int materia, bool editar)
        {
            IdMateria = materia;

            if (editar)
            {
                return RedirectToPage("Materia/CreateMateria");
            }
            else
            {
                try
                {
                    // Eliminar la materia y todas sus relaciones
                    await EliminarMateriaConRelacionesAsync(materia);

                    Materias = await GetMateriasAsync();
                    return RedirectToPage("Materia");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("materia", $"Error al eliminar la materia: {ex.Message}");
                    await OnGetAsync();
                    return Page();
                }
            }
        }

        public async Task EliminarMateriaConRelacionesAsync(int materiaId)
        {
            // Obtener el token JWT
            string token = HttpContext.Session.GetString("JwtToken");

            // Eliminar asistencias asociadas
            var tieneAsistencias = await TieneAsistenciasMateria(materiaId);
            if (tieneAsistencias)
            {
                await EliminarAsistenciasMateria(materiaId, token);
            }

            // Eliminar relaciones con cursos 
            var cursosMaterias = await ObtenerCursosMateriaAsync(materiaId, token);
            if (cursosMaterias.Count > 0)
            {
                await EliminarCursosMateriaAsync(cursosMaterias, token);
            }

            //Eliminar la materia
            await EliminarMateriaAsync(materiaId);
        }

        private async Task EliminarAsistenciasMateria(int materiaId, string token)
        {
            //Obtener todas las asistencias de la materia
            List<Asistencia> asistencias = await ObtenerAsistenciasMateria(materiaId, token);

            if (asistencias.Count == 0)
            {
                return; // No hay asistencias que eliminar
            }

            var asistenciasSimplificadas = asistencias.Select(a => new { Id = a.Id }).ToList();
            var jsonContent = JsonConvert.SerializeObject(asistenciasSimplificadas);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/Asistencia/DeleteAllAsistencia");
            request.Content = content;

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            await _client.SendAsync(request);
        }

        private async Task<List<Asistencia>> ObtenerAsistenciasMateria(int materiaId, string token)
        {
            List<Asistencia> asistencias = new List<Asistencia>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_materia == {materiaId}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Asistencia/GetAsistenciasForCombo?query={queryParam}");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string asistenciasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(asistenciasJson))
                {
                    asistencias = JsonConvert.DeserializeObject<List<Asistencia>>(asistenciasJson);
                }
            }

            return asistencias;
        }

        private async Task<List<CursoMateria>> ObtenerCursosMateriaAsync(int materiaId, string token)
        {
            List<CursoMateria> cursosMaterias = new List<CursoMateria>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_materia == {materiaId}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/CursoMateria/GetCursoMateriaForCombo?query={queryParam}");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string cursosMateriaJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursosMateriaJson))
                {
                    cursosMaterias = JsonConvert.DeserializeObject<List<CursoMateria>>(cursosMateriaJson);
                }
            }

            return cursosMaterias;
        }

        private async Task EliminarCursosMateriaAsync(List<CursoMateria> cursosMaterias, string token)
        {
            var cursosMateriaSimplificados = cursosMaterias.Select(cm => new { Id = cm.Id }).ToList();
            var jsonContent = JsonConvert.SerializeObject(cursosMateriaSimplificados);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/CursoMateria/DeleteAllCursoMateria");
            request.Content = content;

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            await _client.SendAsync(request);
        }
    }
}
