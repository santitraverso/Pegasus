using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class IntegrantesCursosModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public IntegrantesCursosModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<IntegrantesCursos> Alumnos { get; set; } = new List<IntegrantesCursos>();
        [BindProperty]
        public List<IntegrantesCursos> IntegrantesCurso { get; set; } = new List<IntegrantesCursos>();
        [TempData]
        public int IdCurso { get; set; }

        [BindProperty]
        public List<int> SelectedAlumnosIds { get; set; } = new List<int>(); // IDs de los alumnos seleccionados en el formulario


        public async Task OnGetAsync()
        {
            //Traigo todos los usuarios que son alumnos
            var todos = await GetUsuariosAlumnosAsync();

            
            foreach (var alumn in todos)
            {
                IntegrantesCursos alumno = new IntegrantesCursos();
                alumno.Usuario = alumn;
                alumno.Id_Usuario = alumn.Id;
                
                Alumnos.Add(alumno);
            }

            //Traigo los integrantes actuales del curso para marcar en la lista de alumnos
            IntegrantesCurso = await GetIntegrantesCursosAsync(IdCurso);

            foreach (var alumn in Alumnos)
            {
                if (IntegrantesCurso.Any(i => i.Id_Usuario == alumn.Id_Usuario))
                {
                    SelectedAlumnosIds.Add((int)alumn.Id_Usuario);
                }
            }
        }

        public async Task<List<Usuario>> GetUsuariosAlumnosAsync()
        {
            List<Usuario> getalumnos = new List<Usuario>();

            string queryParam = Uri.EscapeDataString("x=>x.id_perfil == 2 && x.activo == true");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Usuario/GetUsuariosForCombo?query={queryParam}");

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
                    getalumnos = JsonConvert.DeserializeObject<List<Usuario>>(alumnosJson);
                }
            }

            return getalumnos;
        }

        public async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();

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
                string alumnosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnosJson))
                {
                    getalumnos = JsonConvert.DeserializeObject<List<IntegrantesCursos>>(alumnosJson);
                }
            }

            return getalumnos;
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
                    // Obtener integrantes actuales
                    IntegrantesCurso = await GetIntegrantesCursosAsync(curso);

                    //Borro los integrantes actuales del curso ya que voy a volver a generarlos con los enviados
                    bool deleteSuccess = await BorrarIntegrantesAsync();

                    if (deleteSuccess)
                    {
                        // Crear nuevos integrantes
                        bool createSuccess = await GuardarIntegrantesAsync(curso, SelectedAlumnosIds);

                        if (createSuccess)
                        {
                            TempData["SuccessMessage"] = "Los integrantes se guardaron correctamente.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Hubo un error al guardar los integrantes.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Hubo un error al eliminar los integrantes existentes.";
                    }

                    return RedirectToPage("Curso");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error: {ex.Message}";
                    return RedirectToPage("Curso");
                }
            }
        }

        public async Task<bool> BorrarIntegrantesAsync()
        {
            try
            {
                // Si no hay integrantes, no hay nada que borrar
                if (IntegrantesCurso == null || !IntegrantesCurso.Any())
                {
                    return true;
                }

                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                // Crear la solicitud para eliminar todos los integrantes de una vez
                var jsonContent = JsonConvert.SerializeObject(IntegrantesCurso);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/IntegrantesCursos/DeleteAllIntegrantesCursos");
                request.Content = content;

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage response = await _client.SendAsync(request);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("curso", $"Error al eliminar integrantes: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GuardarIntegrantesAsync(int curso, List<int> alumnosIds)
        {
            try
            {
                // Si no hay alumnos seleccionados, no hay nada que guardar
                if (alumnosIds == null || !alumnosIds.Any())
                {
                    return true;
                }

                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                // Crear la lista de integrantes para agregar
                var integrantesList = alumnosIds.Select(alumnoId => new IntegrantesCursos
                {
                    Id_Curso = curso,
                    Id_Usuario = alumnoId
                }).ToList();

                // Crear la solicitud para agregar todos los integrantes de una vez
                var jsonContent = JsonConvert.SerializeObject(integrantesList);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/IntegrantesCursos/CreateAllIntegrantesCursos");
                request.Content = content;

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage response = await _client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("curso", "Hubo un error inesperado al agregar alumnos al curso: " + errorResponse);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("curso", $"Error al guardar integrantes: {ex.Message}");
                return false;
            }
        }
    }
}
