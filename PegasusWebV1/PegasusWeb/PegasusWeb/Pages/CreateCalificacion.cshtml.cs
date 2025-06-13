using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text;
using System.Reflection;
using System.Dynamic;
using Microsoft.Extensions.Options;

namespace PegasusWeb.Pages
{
    public class CreateCalificacionModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public CreateCalificacionModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public IntegrantesMaterias Alumno { get; set; } =new IntegrantesMaterias();
        public List<Calificaciones> Calificaciones { get; set; } = new List<Calificaciones>();

        [TempData]
        public int IdIntegrante { get; set; }
        [TempData]
        public bool Nuevo { get; set; }
        [TempData]
        public int Materia { get; set; }
        [TempData]
        public int IdCurso { get; set; }
        [TempData]
        public string? Modulo { get; set; }


        public async Task OnGetAsync()
        {
            if(IdIntegrante == 0 || Materia == 0)
                RedirectToPage("ListaMaterias");

            Alumno = await GenerarAlumno(IdCurso, IdIntegrante, Materia);

            if (Nuevo)
                Alumno.Usuario.Calificaciones = new List<Calificaciones> { };
        }

        private async Task<IntegrantesMaterias> GenerarAlumno(int idCurso, int idIntegrante, int materia)
        {
            var alumno = await GetIntegranteCursoAsync(idCurso, idIntegrante);

            // Crea un nuevo objeto IntegrantesMaterias
            IntegrantesMaterias inte = new IntegrantesMaterias
            {
                Id_Materia = materia,
                Id_Usuario = alumno.Id_Usuario,
                Usuario = alumno.Usuario
            };

            inte.Usuario.Calificaciones = await GetCalificacionesAsync(materia, idCurso, (int)inte.Id_Usuario);

            return inte;
        }

        public async Task<IntegrantesCursos> GetIntegranteCursoAsync(int curso, int usuario)
        {
            IntegrantesCursos getIntegrante = new IntegrantesCursos();
  
            string queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso} && x.id_usuario=={usuario}");
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
                string integranteJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(integranteJson))
                {
                    getIntegrante = JsonConvert.DeserializeObject<List<IntegrantesCursos>>(integranteJson).FirstOrDefault();
                }
            }

            return getIntegrante;
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


        public async Task<IActionResult> OnPostAsync(List<Calificaciones> calificaciones, int alumno, int materia, string calificacionesEliminadas, int curso, bool atras, string modulo)
        {
            Materia = materia;
            IdCurso = curso;
            Modulo = modulo;
            IdIntegrante = alumno;

            if (atras)
            {
                return RedirectToPage("Calificacion");
            }
            else
            {
                // Validar calificaciones
                foreach (var calificacion in calificaciones)
                {
                    if (calificacion.Calificacion < 1 || calificacion.Calificacion > 10)
                    {
                        ModelState.AddModelError("nota", "El campo Nota es requerido");
                        await OnGetAsync();
                        return Page();
                    }
                }

                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                // Procesar eliminaciones masivas
                if (!string.IsNullOrEmpty(calificacionesEliminadas))
                {
                    var idsEliminados = calificacionesEliminadas.Split(',').Select(int.Parse).ToList();

                    if (idsEliminados.Any())
                    {
                        // Crear lista de objetos Calificaciones para eliminar
                        var calificacionesParaEliminar = idsEliminados.Select(id => new Calificaciones { Id = id }).ToList();

                        var jsonContent = JsonConvert.SerializeObject(calificacionesParaEliminar);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/Calificaciones/DeleteAllCalificaciones");
                        request.Content = content;

                        if (!string.IsNullOrEmpty(token))
                        {
                            request.Headers.Add("Authorization", $"Bearer {token}");
                        }

                        var response = await _client.SendAsync(request);

                        if (!response.IsSuccessStatusCode)
                        {
                            var errorResponse = await response.Content.ReadAsStringAsync();
                            ModelState.AddModelError("calificacion", "Hubo un error al eliminar las calificaciones: " + errorResponse);
                            await OnGetAsync();
                            return Page();
                        }
                    }
                }

                // Preparar calificaciones para crear y actualizar
                var calificacionesParaCrear = new List<dynamic>();
                var calificacionesParaActualizar = new List<dynamic>();

                foreach (var calificacion in calificaciones)
                {
                    dynamic calificacionData = new ExpandoObject();
                    calificacionData.Calificacion = calificacion.Calificacion;
                    calificacionData.Id_Materia = materia;
                    calificacionData.Id_Curso = curso;
                    calificacionData.Id_Alumno = alumno;

                    if (calificacion.Id > 0)
                    {
                        calificacionData.Id = calificacion.Id;
                        calificacionesParaActualizar.Add(calificacionData);
                    }
                    else
                    {
                        calificacionesParaCrear.Add(calificacionData);
                    }
                }

                // Procesar creaciones masivas
                if (calificacionesParaCrear.Any())
                {
                    var jsonContent = JsonConvert.SerializeObject(calificacionesParaCrear);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/Calificaciones/CreateAllCalificaciones");
                    request.Content = content;

                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Add("Authorization", $"Bearer {token}");
                    }

                    var response = await _client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("calificacion", "Hubo un error al crear las calificaciones: " + errorResponse);
                        await OnGetAsync();
                        return Page();
                    }
                }

                // Procesar actualizaciones masivas
                if (calificacionesParaActualizar.Any())
                {
                    var jsonContent = JsonConvert.SerializeObject(calificacionesParaActualizar);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/Calificaciones/UpdateAllCalificaciones");
                    request.Content = content;

                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Add("Authorization", $"Bearer {token}");
                    }

                    var response = await _client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("calificacion", "Hubo un error al actualizar las calificaciones: " + errorResponse);
                        await OnGetAsync();
                        return Page();
                    }
                }

                TempData["SuccessMessage"] = "La calificación se guardó correctamente.";
                return RedirectToPage("Calificacion");
            }
        }

    }
}
