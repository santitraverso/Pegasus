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
    public class CreateComunicadoModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public CreateComunicadoModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        [BindProperty]
        public CuadernoComunicados? Comunicado { get; set; }

        [TempData]
        public int IdComunicado { get; set; }

        [TempData]
        public int IdCurso { get; set; }
        [TempData]
        public string? Modulo { get; set; }
        [TempData]
        public int IdUsuario { get; set; }

        [TempData]
        public string? IdsAlumnosJson { get; set; }

        [BindProperty]
        public string? NombresConcatenados { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (IdComunicado > 0)
            {

                Comunicado = await GetComunicadoCursoAsync(IdComunicado);

                var comunicadoAlumnos = await GetAlumnosComunicadoAsync(IdComunicado);

                NombresConcatenados = string.Join(", ", comunicadoAlumnos.Select(a => a.Alumno.Apellido + ' ' + a.Alumno.Nombre));

                if (Comunicado == null)
                {
                    return NotFound();
                }
            }
            else
            {
                Comunicado = new CuadernoComunicados { Id = 0 };

                string nombresConcatenados = "";

                List<int> idsAlumnos = new List<int>();

                if (!string.IsNullOrEmpty(IdsAlumnosJson))
                {
                    idsAlumnos = JsonConvert.DeserializeObject<List<int>>(IdsAlumnosJson);
                }

                foreach (var idAlumno in idsAlumnos)
                {
                    Usuario usu = await GetUsuarioAsync(idAlumno);

                    nombresConcatenados += $"{usu.Apellido} {usu.Nombre}, ";
                }

                // Remover la última coma y espacio si es necesario
                if (nombresConcatenados.EndsWith(", "))
                {
                    nombresConcatenados = nombresConcatenados.Substring(0, nombresConcatenados.Length - 2);
                }

                NombresConcatenados = nombresConcatenados;
            }

            return Page();
        }

        private async Task<List<ComunicadoAlumnos>> GetAlumnosComunicadoAsync(int idComunicado)
        {
            List<ComunicadoAlumnos> getusuarios = new List<ComunicadoAlumnos>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_comunicado == {idComunicado}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/ComunicadoAlumnos/GetComunicadoAlumnossForCombo?query={queryParam}");

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
                    getusuarios = JsonConvert.DeserializeObject<List<ComunicadoAlumnos>>(usuariosJson);
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


        public async Task<IActionResult> OnPostAsync(bool atras, int curso, int usuario, string ids, string descripcion, string modulo, int id)
        {
            IdCurso = curso;
            Modulo = modulo;
            IdComunicado = id;
            IdUsuario = usuario;
            bool nuevo = id == 0;

            string trimmedIds = ids.Trim('[', ']');
            string[] idsArray = trimmedIds.Split(',');
            List<int> idsAlumnos = idsArray.Select(id => int.Parse(id)).ToList();

            if (atras)
            {
                return RedirectToPage("Cuaderno");
            }
            else
            {
                if (string.IsNullOrEmpty(descripcion))
                {
                    this.ModelState.AddModelError("descripcion", "El campo Descripcion es requerido");
                }

                ModelState.Remove(nameof(NombresConcatenados));

                if (!ModelState.IsValid)
                {
                    // Recuperar los errores de ModelState
                    var errors = ModelState.Values.SelectMany(v => v.Errors);

                    foreach (var error in errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                    await OnGetAsync();
                    return Page();
                }

                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                // Preparar datos del comunicado
                dynamic comunicadoData = new ExpandoObject();
                comunicadoData.Id_Usuario = usuario;
                comunicadoData.Id_Curso = curso;
                comunicadoData.Descripcion = descripcion;
                comunicadoData.Fecha = DateTime.Now;

                if (id > 0)
                {
                    comunicadoData.Id = id;
                }

                var jsonContent = JsonConvert.SerializeObject(comunicadoData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
                HttpResponseMessage response;
                if (id > 0)
                {
                    var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/CuadernoComunicados/UpdateCuadernoComunicados");
                    request.Content = content;

                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Add("Authorization", $"Bearer {token}");
                    }

                    response = await _client.SendAsync(request);
                }
                else
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/CuadernoComunicados/CreateCuadernoComunicados");
                    request.Content = content;

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
                    ModelState.AddModelError("comunicado", id > 0
                        ? "Hubo un error inesperado al actualizar el Comunicado: " + errorResponse
                        : "Hubo un error inesperado al crear el Comunicado: " + errorResponse);

                    await OnGetAsync();
                    return Page();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var comunicadoCreado = JsonConvert.DeserializeObject<dynamic>(responseContent);
                id = comunicadoCreado.id;

                // Si es un nuevo comunicado, crear las relaciones con los alumnos en una sola llamada
                if (nuevo && idsAlumnos.Any())
                {
                    var comunicadoAlumnosList = new List<dynamic>();

                    foreach (var idAlumno in idsAlumnos)
                    {
                        dynamic comunicadoAlumnoData = new ExpandoObject();
                        comunicadoAlumnoData.Id_Comunicado = id;
                        comunicadoAlumnoData.Id_Alumno = idAlumno;

                        comunicadoAlumnosList.Add(comunicadoAlumnoData);
                    }

                    var jsonContentAlumnos = JsonConvert.SerializeObject(comunicadoAlumnosList);
                    var contentAlumnos = new StringContent(jsonContentAlumnos, Encoding.UTF8, "application/json");

                    var requestAlumnos = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/ComunicadoAlumnos/CreateAllComunicadoAlumnos");
                    requestAlumnos.Content = contentAlumnos;

                    if (!string.IsNullOrEmpty(token))
                    {
                        requestAlumnos.Headers.Add("Authorization", $"Bearer {token}");
                    }

                    var responseAlumnos = await _client.SendAsync(requestAlumnos);

                    // Manejar errores
                    if (!responseAlumnos.IsSuccessStatusCode)
                    {
                        var errorResponse = await responseAlumnos.Content.ReadAsStringAsync();
                        ModelState.AddModelError("comunicado", "Hubo un error inesperado al crear las relaciones entre Comunicado y Alumnos: " + errorResponse);
                        await OnGetAsync();
                        return Page();
                    }
                }

                TempData["SuccessMessage"] = "El Comunicado se guardó correctamente.";
                return RedirectToPage("Cuaderno");
            }
        }
    }
}
