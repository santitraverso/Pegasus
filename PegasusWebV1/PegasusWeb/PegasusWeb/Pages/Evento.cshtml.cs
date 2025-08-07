using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class EventoModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public EventoModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<Evento>? Eventos { get; set; }

        [TempData]
        public int IdEvento { get; set; }
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

            var eventos = await GetEventosAsync(IdPerfil);
            Eventos = eventos.Where(e => e.Fecha >= DateTime.Today).OrderByDescending(e => e.Fecha).ToList();

            return Page();
        }

        public async Task<List<Evento>> GetEventosAsync(long idPerfil)
        {
            List<Evento> geteventos = new List<Evento>();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Evento/GetEventosForCombo");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string eventosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(eventosJson))
                {
                    var todosLosEventos = JsonConvert.DeserializeObject<List<Evento>>(eventosJson);

                    // Filtrar eventos según el perfil del usuario
                    geteventos = FiltrarEventosPorPerfil(todosLosEventos, idPerfil);
                }

            }

            return geteventos;
        }

        private static List<Evento> FiltrarEventosPorPerfil(List<Evento> eventos, long idPerfil)
        {
            return idPerfil switch
            {
                (int)TipoPerfil.Alumno => eventos.Where(e => e.TipoDestinatario == TipoDestinatario.Alumnos ||
                                       e.TipoDestinatario == TipoDestinatario.Ambos).ToList(),
                (int)TipoPerfil.Padre => eventos.Where(e => e.TipoDestinatario == TipoDestinatario.Padres ||
                                       e.TipoDestinatario == TipoDestinatario.Ambos).ToList(), 
                _ => eventos // Para otros perfiles, mostrar todos
            };
        }

        public async Task<IActionResult> OnPostAsync(int evento, int usuario, int perfil, bool editar, bool confirmar, bool asistencia)
        {
            IdEvento= evento;
            IdUsuario= usuario;
            IdPerfil= perfil;

            if(confirmar)
                return RedirectToPage("ConfirmarAsistencia");
            if (asistencia)
                return RedirectToPage("ConfirmacionesEvento");

            if (editar)
            {
                return RedirectToPage("CreateEvento");
            }
            else
            {
                await EliminarIntegrantesEventoAsync(evento);
                await EliminarEventoAsync(evento);
                await OnGetAsync();
                return RedirectToPage("Evento");
            }
        }

        public async Task EliminarEventoAsync(int evento)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/Evento/DeleteEvento/{evento}");

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
                this.ModelState.AddModelError("evento", "Hubo un error inesperado al borrar el Evento " + errorResponse);
            }
        }

        public async Task EliminarIntegrantesEventoAsync(int evento)
        {
            try
            {
                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                // Obtener todos los integrantes del evento
                string queryParam = Uri.EscapeDataString($"x=>x.id_evento=={evento}");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/IntegrantesEventos/GetIntegrantesEventossForCombo?query={queryParam}");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage integrantesResponse = await _client.SendAsync(request);

                if (integrantesResponse.IsSuccessStatusCode)
                {
                    var integrantesJson = await integrantesResponse.Content.ReadAsStringAsync();
                    var integrantes = JsonConvert.DeserializeObject<List<IntegrantesEventos>>(integrantesJson);

                    if (integrantes != null && integrantes.Any())
                    {
                        var jsonContent = JsonConvert.SerializeObject(integrantes);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/IntegrantesEventos/DeleteAllIntegrantesEventos");
                        deleteRequest.Content = content;

                        if (!string.IsNullOrEmpty(token))
                        {
                            deleteRequest.Headers.Add("Authorization", $"Bearer {token}");
                        }

                        var deleteResponse = await _client.SendAsync(deleteRequest);

                        if (!deleteResponse.IsSuccessStatusCode)
                        {
                            var errorResponse = await deleteResponse.Content.ReadAsStringAsync();
                            ModelState.AddModelError("evento", "Hubo un error inesperado al eliminar los integrantes del Evento: " + errorResponse);
                        }
                    }
                }
                else
                {
                    var errorResponse = await integrantesResponse.Content.ReadAsStringAsync();
                    ModelState.AddModelError("evento", "Hubo un error inesperado al buscar los integrantes del Evento: " + errorResponse);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("evento", $"Error al eliminar integrantes del evento: {ex.Message}");
            }
        }
    }
}
