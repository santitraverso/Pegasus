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
    public class CreateContenidoMateriasModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public CreateContenidoMateriasModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        [BindProperty]
        public ContenidoMaterias? Contenido { get; set; }

        [TempData]
        public int IdContenido { get; set; }
        [TempData]
        public int IdMateria { get; set; }

        [BindProperty]
        public int Materia { get; set; }

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

            if (IdContenido > 0)
            {

                Contenido = await GetContenidoMateriasAsync(IdContenido);

                if (Contenido == null)
                {
                    return NotFound();
                }
            }
            else
            {
                Contenido = new ContenidoMaterias { Id = 0 };
            }

            Materia = IdMateria;
            return Page();
        }

        async Task<ContenidoMaterias> GetContenidoMateriasAsync(int contenido)
        {
            ContenidoMaterias getcontenido = new ContenidoMaterias();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/ContenidoMaterias/GetById?id={contenido}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string contenidoJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(contenidoJson))
                {
                    getcontenido = JsonConvert.DeserializeObject<ContenidoMaterias>(contenidoJson);
                }
            }

            return getcontenido;
        }


        public async Task<IActionResult> OnPostAsync(bool atras, int materia, string titulo, string descripcion, int id)
        {
            if (atras)
            {
                IdMateria = materia;
                return RedirectToPage("ListaContenidos");
            }
            else
            {
                if (materia < 1)
                {
                    this.ModelState.AddModelError("materia", "El campo Materia es requerido");
                }
                if (string.IsNullOrEmpty(titulo))
                {
                    this.ModelState.AddModelError("titulo", "El campo Titulo es requerido");
                }
                if (string.IsNullOrEmpty(descripcion))
                {
                    this.ModelState.AddModelError("descripcion", "El campo Descripcion es requerido");
                }

                if (!ModelState.IsValid)
                {
                    IdContenido = id;
                    await OnGetAsync();
                    return Page();
                }


                dynamic contenidoData = new ExpandoObject();
                contenidoData.Id_Materia = materia;
                contenidoData.Titulo = titulo;
                contenidoData.Descripcion = descripcion;

                if (id > 0)
                {
                    contenidoData.Id = id;
                }

                // Convertir el objeto dinámico a JSON
                var jsonContent = JsonConvert.SerializeObject(contenidoData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
                HttpResponseMessage response;
                if (id > 0)
                {
                    var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/ContenidoMaterias/UpdateContenidoMaterias");

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
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/ContenidoMaterias/CreateContenidoMaterias");

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
                    ModelState.AddModelError("contenido", id > 0
                        ? "Hubo un error inesperado al actualizar el Contenido: " + errorResponse
                        : "Hubo un error inesperado al crear el Contenido: " + errorResponse);

                    await GetContenidoMateriasAsync(id);
                    return Page();
                }

                IdMateria = materia;
                TempData["SuccessMessage"] = "El Contenido se guardó correctamente.";
                return RedirectToPage("ListaContenidos");
            }
        }
    }
}
