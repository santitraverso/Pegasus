using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System;
using System.Diagnostics.Contracts;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ConfirmacionesEventoModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public ConfirmacionesEventoModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        [BindProperty]
        public List<IntegrantesEventos>? Integrantes { get; set; }
        [BindProperty]
        public Evento? Evento { get; set; }
        [TempData]
        public int IdEvento { get; set; }
        [TempData]
        public int IdPerfil { get; set; }
        [TempData]
        public int IdUsuario { get; set; }
        

        public async Task<IActionResult> OnGetAsync()
        {
            if (IdEvento > 0)
            {
                Evento = await GetEventoAsync(IdEvento);

                Integrantes = await GetIntegrantesEventoAsync(IdEvento);

                if (Integrantes == null)
                {
                    return NotFound();
                }
            }
            else
            {
                Integrantes = new List<IntegrantesEventos> ();
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

        public async Task<List<IntegrantesEventos>> GetIntegrantesEventoAsync(long evento)
        {
            List<IntegrantesEventos> getIntegrantes = new();

            string queryParam = Uri.EscapeDataString($"x=>x.id_evento=={evento}");
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
                string integrantesJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(integrantesJson))
                {
                    getIntegrantes = JsonConvert.DeserializeObject<List<IntegrantesEventos>>(integrantesJson);
                }
            }

            return getIntegrantes;
        }

        public IActionResult OnPostAsync(int evento, int usuario, int perfil)
        {
            IdEvento= evento;
            IdUsuario= usuario;
            IdPerfil= perfil;

            return RedirectToPage("Evento");
        }
    }
}
