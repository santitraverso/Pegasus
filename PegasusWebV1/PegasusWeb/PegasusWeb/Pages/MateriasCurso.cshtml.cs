using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class MateriasCursoModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public MateriasCursoModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<Entities.Materia> Materias { get; set; } = new List<Entities.Materia>();
        [BindProperty]
        public List<CursoMateria> MateriasCurso { get; set; } = new List<CursoMateria>();
        [TempData]
        public int IdCurso { get; set; }

        [BindProperty]
        public List<int> SelectedMateriasIds { get; set; } = new List<int>();


        public async Task OnGetAsync()
        {
            Materias = await GetMateriasAsync();

            //Traigo las materias actuales del curso para marcar en la lista de materias
            MateriasCurso = await GetMateriasCursoAsync(IdCurso);

            foreach (var mat in Materias)
            {
                if (MateriasCurso.Any(i => i.Id_Materia == mat.Id))
                {
                    SelectedMateriasIds.Add((int)mat.Id);
                }
            }
        }

        async Task<List<Entities.Materia>> GetMateriasAsync()
        {
            List<Entities.Materia> getMaterias = new List<Entities.Materia>();

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

        public async Task<IActionResult?> OnPost(int curso, bool atras)
        {
            if (atras)
            {
                IdCurso = curso;
                return RedirectToPage("CreateCurso");
            }
            else
            {
                try
                {
                    // Obtener materias actuales del curso
                    MateriasCurso = await GetMateriasCursoAsync(curso);

                    // Eliminar todas las materias actuales del curso
                    bool deleteSuccess = await BorrarMateriasAsync();

                    if (deleteSuccess)
                    {
                        // Crear nuevas relaciones curso-materia
                        var cursoMateriaList = SelectedMateriasIds.Select(materiaId => new CursoMateria
                        {
                            Id_Curso = curso,
                            Id_Materia = materiaId
                        }).ToList();

                        bool createSuccess = await GuardarMateriasMasivoAsync(cursoMateriaList);

                        if (!createSuccess)
                        {
                            TempData["ErrorMessage"] = "Hubo un error al guardar las materias.";
                            return RedirectToPage("Curso");
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Hubo un error al eliminar las materias existentes.";
                        return RedirectToPage("Curso");
                    }

                    TempData["SuccessMessage"] = "Las materias se guardaron correctamente.";
                    return RedirectToPage("Curso");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error: {ex.Message}";
                    return RedirectToPage("Curso");
                }
            }
        }

        public async Task<bool> BorrarMateriasAsync()
        {
            try
            {
                // Si no hay materias, no hay nada que borrar
                if (MateriasCurso == null || !MateriasCurso.Any())
                {
                    return true;
                }

                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                var cursoMateriaList = MateriasCurso.Select(cm => new { Id = cm.Id }).ToList();

                var jsonContent = JsonConvert.SerializeObject(cursoMateriaList);
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

        public async Task<bool> GuardarMateriasMasivoAsync(List<CursoMateria> cursoMaterias)
        {
            try
            {
                // Si no hay materias, no hay nada que guardar
                if (cursoMaterias == null || !cursoMaterias.Any())
                {
                    return true;
                }

                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                var jsonContent = JsonConvert.SerializeObject(cursoMaterias);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/CursoMateria/CreateAllCursoMateria");
                request.Content = content;

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage response = await _client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("curso", $"Error al guardar materias: {errorResponse}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("curso", $"Error al guardar materias: {ex.Message}");
                return false;
            }
        }
    }
}
