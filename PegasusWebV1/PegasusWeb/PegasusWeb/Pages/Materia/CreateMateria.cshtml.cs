using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PegasusWeb.Pages
{
    public class CreateMateriaModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public CreateMateriaModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        [DisplayName("Curso"), Required(ErrorMessage = "El campo Curso es requerido")]
        public int idCurso { get; set; }

        [DisplayName("Nombre de la materia"), Required(ErrorMessage = "El campo Nombre de la materia es requerido")]
        public string? Nombre { get; set; }

        [DisplayName("Materia"), Required(ErrorMessage = "Hubo un error inesperado creando la Materia")]
        public string? materia { get; set; }

        [BindProperty]
        public Entities.Materia? Materia { get; set; }

        [TempData]
        public int IdMateria { get; set; }

        public List<ContenidoMaterias> Contenidos { get; set; } = new List<ContenidoMaterias>();

        [BindProperty]
        public int CursoSeleccionadoId { get; set; }

        public List<SelectListItem> CursosRelacionados { get; set; } = new List<SelectListItem> { };
        [TempData]
        public int IdPerfil { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;

            if (IdMateria > 0)
            {
                // Es una edición, se carga el curso existente
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Materia/GetById?id={IdMateria}");

                // Añadir el token JWT al encabezado
                string token = HttpContext.Session.GetString("JwtToken");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string materiaJson = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(materiaJson))
                    {
                        Materia = JsonConvert.DeserializeObject<Entities.Materia>(materiaJson);

                        Contenidos = await GetContenidosMateriaAsync(IdMateria);
                    }
                }

                if (Materia == null)
                {
                    return NotFound();
                }
            }
            else
            {
                // Es una carga nueva
                Materia = new Entities.Materia { Id = 0 };
            }

            return Page();
        }

        public IActionResult OnPostModificarContenido(int materia)
        {
            IdMateria = materia;
            return RedirectToPage("../ListaContenidos");
        }

        async Task<List<ContenidoMaterias>> GetContenidosMateriaAsync(int materia)
        {
            List<ContenidoMaterias> getContenidos = new List<ContenidoMaterias>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_materia=={materia}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/ContenidoMaterias/GetContenidoMateriasForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string contenidosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(contenidosJson))
                {
                    getContenidos = JsonConvert.DeserializeObject<List<ContenidoMaterias>>(contenidosJson);
                }
            }

            return getContenidos;
        }

        public async Task<IActionResult> OnPost(string nombre, int id, bool atras)
        {
            if (atras)
            {
                IdMateria = id;
                return RedirectToPage("../Materia");
            }
            else
            {

                if (string.IsNullOrEmpty(nombre))
                {
                    this.ModelState.AddModelError("nombreMateria", "El campo Nombre de la materia es requerido");
                }

                if (!ModelState.IsValid)
                {
                    return Page();
                }


                dynamic materiaData = new ExpandoObject();
                materiaData.Nombre = nombre;

                if (id > 0)
                {
                    materiaData.Id = id;
                }

                // Convertir el objeto dinámico a JSON
                var jsonContent = JsonConvert.SerializeObject(materiaData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
                HttpResponseMessage response;
                if (id > 0)
                {
                    var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/Materia/UpdateMateria");

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
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/Materia/CreateMateria");

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
                    ModelState.AddModelError("materia", id > 0
                        ? "Hubo un error inesperado al actualizar la Materia: " + errorResponse
                        : "Hubo un error inesperado al crear la Materia: " + errorResponse);

                    await OnGetAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = "La Materia se guardó correctamente.";
                return RedirectToPage("../Materia");
            }
        }
    }
}
