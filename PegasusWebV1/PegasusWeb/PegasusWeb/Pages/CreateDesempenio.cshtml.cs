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
using System.Globalization;
using Microsoft.Extensions.Options;

namespace PegasusWeb.Pages
{
    public class CreateDesempenioModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public CreateDesempenioModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        [BindProperty]
        public DesempenioAlumnos? Desempenio { get; set; }

        [TempData]
        public int IdDesempenio { get; set; }

        [TempData]
        public int IdCurso { get; set; }
        [TempData]
        public string? Modulo { get; set; }

        [TempData]
        public bool Ver { get; set; }
        [TempData]
        public int IdAlumno { get; set; }

        [BindProperty]
        public Usuario? Alumno { get; set; }
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

            if (IdDesempenio > 0)
            {
                Desempenio = await GetDesempenioCursoAsync(IdDesempenio);

                if (Desempenio == null)
                {
                    return NotFound();
                }
            }
            else
            {
                Desempenio = new DesempenioAlumnos { Id = 0 };
            }

            Alumno = await GetUsuarioAsync(IdAlumno);

            return Page();
        }

        private async Task<DesempenioAlumnos> GetDesempenioCursoAsync(int idDesempenio)
        {
            DesempenioAlumnos getusuarios = new DesempenioAlumnos();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/DesempenioAlumnos/GetById?id={idDesempenio}");

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
                    getusuarios = JsonConvert.DeserializeObject<DesempenioAlumnos>(usuariosJson);
                }
            }

            return getusuarios;
        }

        async Task<Usuario> GetUsuarioAsync(int usuario)
        {
            Usuario getusuario = new Usuario();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Usuario/GetById?id={usuario}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string usuarioJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(usuarioJson))
                {
                    getusuario = JsonConvert.DeserializeObject<Usuario>(usuarioJson);
                }
            }

            return getusuario;
        }

        async Task<CuadernoComunicados> GetComunicadoCursoAsync(int comunicado)
        {
            CuadernoComunicados getcomunicado = new CuadernoComunicados();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/CuadernoComunicados/GetById?id={comunicado}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string comunicadoJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(comunicadoJson))
                {
                    getcomunicado = JsonConvert.DeserializeObject<CuadernoComunicados>(comunicadoJson);
                }
            }

            return getcomunicado;
        }


        public async Task<IActionResult> OnPostAsync(bool atras, int curso, bool ver, string modulo, int id, string asistencia, string participacion, string calificaciones, string tareas, int alumno)
        {
            IdCurso = curso;
            Modulo = modulo;
            IdDesempenio = id;
            Ver = ver;
            decimal.TryParse(asistencia, NumberStyles.Any, CultureInfo.InvariantCulture, out var asistenciaDecimal);
            decimal.TryParse(participacion, NumberStyles.Any, CultureInfo.InvariantCulture, out var participacionDecimal);
            decimal.TryParse(calificaciones, NumberStyles.Any, CultureInfo.InvariantCulture, out var calificacionesDecimal);
            decimal.TryParse(tareas, NumberStyles.Any, CultureInfo.InvariantCulture, out var tareasDecimal);

            if (atras)
            {
                return RedirectToPage("Desempenio");
            }
            else
            {
                if(ver)
                {
                    await EliminarDesempenioAlumnoAsync(id);
                    if (this.ModelState.IsValid)
                        return RedirectToPage("Desempenio");
                    else
                        await OnGetAsync();
                    return Page();
                }
                else
                {
                    if (asistenciaDecimal < 1 || participacionDecimal < 1 || calificacionesDecimal < 1 || tareasDecimal < 1)
                    {
                        this.ModelState.AddModelError("desempenio", "Todos los campos deben tener un valor.");
                    }

                    if (asistenciaDecimal > 10 || participacionDecimal > 10 || calificacionesDecimal > 10 || tareasDecimal > 10)
                    {
                        this.ModelState.AddModelError("desempenio", "Los campos no pueden tener un valor mayor a 10.");
                    }

                    if (!ModelState.IsValid)
                    {
                        foreach (var state in ModelState)
                        {
                            string fieldName = state.Key;
                            var fieldErrors = state.Value.Errors;

                            foreach (var error in fieldErrors)
                            {
                                Console.WriteLine($"Error en el campo '{fieldName}': {error.ErrorMessage}");
                            }
                        }
                        await OnGetAsync();
                        return Page();
                    }

                    decimal promedio = Math.Round((asistenciaDecimal + participacionDecimal + calificacionesDecimal + tareasDecimal) / 4, 2);

                    dynamic desempenioData = new ExpandoObject();
                    desempenioData.Id_Alumno = alumno;
                    desempenioData.Participacion = participacionDecimal;
                    desempenioData.Asistencia = asistenciaDecimal;
                    desempenioData.Tareas = tareasDecimal;
                    desempenioData.Calificaciones = calificacionesDecimal;
                    desempenioData.Promedio = promedio;
                    desempenioData.Id_Curso = curso;


                    if (id > 0)
                    {
                        desempenioData.Id = id;
                    }

                    // Convertir el objeto dinámico a JSON
                    var jsonContent = JsonConvert.SerializeObject(desempenioData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
                    HttpResponseMessage response;
                    if (id > 0)
                    {
                        var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/DesempenioAlumnos/UpdateDesempenioAlumnos");

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
                        var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/DesempenioAlumnos/CreateDesempenioAlumnos");

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
                        ModelState.AddModelError("desempenio", id > 0
                            ? "Hubo un error inesperado al actualizar el Desempenio: " + errorResponse
                            : "Hubo un error inesperado al crear el Desempenio: " + errorResponse);

                        await OnGetAsync();
                        return Page();
                    }

                    TempData["SuccessMessage"] = "El Desempenio se guardó correctamente.";
                    return RedirectToPage("Desempenio");
                }
            }
        }

        public async Task EliminarDesempenioAlumnoAsync(int desempenio)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/DesempenioAlumnos/DeleteDesempenioAlumnos/{desempenio}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("desempeno", "Hubo un error inesperado al borrar el Desempeño");
            }
        }
    }
}
