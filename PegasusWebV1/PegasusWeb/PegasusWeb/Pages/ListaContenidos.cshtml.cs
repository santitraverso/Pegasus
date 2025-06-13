using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ListaContenidosModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public ListaContenidosModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<ContenidoMaterias> Contenidos { get; set; } = new List<ContenidoMaterias> { };

        [TempData]
        public int IdMateria { get; set; }
        [TempData]
        public int IdContenido { get; set; }
        public async Task OnGetAsync()
        {
            Contenidos = await GetContenidosAsync(IdMateria);
        }

        async Task<List<ContenidoMaterias>> GetContenidosAsync(int materia)
        {
            List<ContenidoMaterias> getcontenidos = new List<ContenidoMaterias>();

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
                    getcontenidos = JsonConvert.DeserializeObject<List<ContenidoMaterias>>(contenidosJson);
                }
            }

            return getcontenidos;
        }

        public async Task<IActionResult> OnPostAsync(int contenido, bool editar, int materia)
        {
            IdContenido = contenido;
            IdMateria = materia;
            
            if (editar)
            {
                return RedirectToPage("CreateContenidoMaterias");
            }
            else
            {
                await EliminarContenidoMateriasAsync(contenido);
                Contenidos = await GetContenidosAsync(IdMateria);
                return RedirectToPage("ListaContenidos");
            }
            
        }

        public IActionResult OnPostAgregarContenido(int materia, bool atras)
        {
            IdMateria = materia;

            if (atras)
            {
                return RedirectToPage("Materia/CreateMateria");
            }
            else
            {
                return RedirectToPage("CreateContenidoMaterias");
            }
            
        }

        public async Task EliminarContenidoMateriasAsync(int contenido)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/ContenidoMaterias/DeleteContenidoMaterias/{contenido}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("calificacion", "Hubo un error inesperado al borrar el Contenido");
            }
        }
    }
}
