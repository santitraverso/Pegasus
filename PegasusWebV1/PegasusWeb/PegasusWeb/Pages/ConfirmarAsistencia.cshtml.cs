using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PegasusWeb.Entities;
using System;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ConfirmarAsistenciaModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public ConfirmarAsistenciaModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        [BindProperty]
        public Evento? Evento { get; set; }
        [BindProperty]
        public IntegrantesEventos? Integrante { get; set; }
        [TempData]
        public int IdEvento { get; set; }
        [TempData]
        public int IdPerfil { get; set; }
        [TempData]
        public int IdUsuario { get; set; }
        [BindProperty]
        public bool Confirmacion { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            if (IdEvento > 0)
            {

                Evento = await GetEventoAsync(IdEvento);

                if (Evento == null)
                {
                    return NotFound();
                }

                // Verificar si ya confirmó
                Integrante = await GetIntegranteEventoAsync(IdEvento, IdUsuario);

                if (Integrante != null)
                {
                    Confirmacion = Integrante.Confirmado;

                    // Marcar como leído
                    if(!Integrante.Leido)
                        await MarcarComoLeidoAsync(Integrante.Id, IdEvento, IdUsuario);
                }
            }
            else
            {
                Evento = new Evento { Id = 0 };
            }

            return Page();
        }

        public async Task<Evento> GetEventoAsync(long idEvento)
        {
            Evento getEvento = new Evento();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Evento/GetById?id={idEvento}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string eventoJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(eventoJson))
                {
                    getEvento = JsonConvert.DeserializeObject<Evento>(eventoJson);
                }
            }

            return getEvento;
        }

        public async Task<IntegrantesEventos> GetIntegranteEventoAsync(long evento, long usuario)
        {
            IntegrantesEventos getIntegrante = new IntegrantesEventos();

            string queryParam = Uri.EscapeDataString($"x=>x.id_evento=={evento} && x.id_usuario=={usuario}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/IntegrantesEventos/GetIntegrantesEventossForCombo?query={queryParam}");

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
                    getIntegrante = JsonConvert.DeserializeObject<List<IntegrantesEventos>>(integranteJson).FirstOrDefault();
                }
            }

            return getIntegrante;
        }

        private async Task MarcarComoLeidoAsync(int id, int eventoId, int usuarioId)
        {
            dynamic eventoData = new ExpandoObject();
            eventoData.Id = id;
            eventoData.Id_Evento = eventoId;
            eventoData.Id_Usuario = usuarioId;
            eventoData.Leido = true;
            eventoData.Confirmado = false;

            // Convertir el objeto dinámico a JSON
            var jsonContent = JsonConvert.SerializeObject(eventoData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/IntegrantesEventos/UpdateIntegrantesEventos");

            request.Content = content;

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
                ModelState.AddModelError("asistencia", "Hubo un error inesperado al actualizar la Lectura: " + errorResponse);
            }

        }

        private async Task ActualizarConfirmacionAsync(int id, int eventoId, int usuarioId, bool confirmar)
        {
            dynamic eventoData = new ExpandoObject();
            eventoData.Id = id;
            eventoData.Id_Evento = eventoId;
            eventoData.Id_Usuario = usuarioId;
            eventoData.Leido = true;
            eventoData.Confirmado = confirmar;

            // Convertir el objeto dinámico a JSON
            var jsonContent = JsonConvert.SerializeObject(eventoData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/IntegrantesEventos/UpdateIntegrantesEventos");

            request.Content = content;

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            // Manejar errores de la respuesta HTTP
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("asistencia", "Hubo un error inesperado al actualizar la Asistencia: " + errorResponse);
            }
        }


        public async Task<IActionResult> OnPostAsync(int id, int evento, int usuario, int perfil, bool confirmar, bool atras)
        {
            IdEvento= evento;
            IdUsuario= usuario;
            IdPerfil= perfil;
            Confirmacion = confirmar;

            if (atras)
            {
                return RedirectToPage("Evento");
            }

            await ActualizarConfirmacionAsync(id, IdEvento, IdUsuario, Confirmacion);

            TempData["SuccessMessage"] = confirmar ?
                "Asistencia confirmada correctamente" :
                "Se ha registrado que no asistirás";

            return RedirectToPage("Evento");
        }
    }
}
