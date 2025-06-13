using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ContactoModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public ContactoModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<Contactos>? Contactos { get; set; }

        [TempData]
        public int TipoContacto { get; set; }

        [TempData]
        public int IdContacto { get; set; }
        [TempData]
        public int IdPerfil { get; set; }


        public async Task OnGetAsync()
        {
            IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;
            Contactos = await GetContactosAsync(TipoContacto);
        }

        async Task<List<Contactos>> GetContactosAsync(int tipoContacto)
        {
            List<Contactos> getcontactos = new List<Contactos>();

            string queryParam = Uri.EscapeDataString($"x=>x.tipo_contacto=={tipoContacto}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Contactos/GetContactosForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string contactosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(contactosJson))
                {
                    getcontactos = JsonConvert.DeserializeObject<List<Contactos>>(contactosJson);
                }
            }

            return getcontactos;
        }

        public async Task<IActionResult> OnPostAsync(int tipoContacto, bool editar, int contacto)
        {
            IdContacto = contacto;
   
            if (editar)
            {
                return RedirectToPage("CreateContacto");
            }
            else
            {
                TipoContacto = tipoContacto;
                await EliminarContactoAsync(contacto);
                return RedirectToPage("Contacto");
            }
        }

        public async Task EliminarContactoAsync(int contacto)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/Contactos/DeleteContacto/{contacto}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);


            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("contacto", "Hubo un error inesperado al borrar el Contacto");
            }
        }
    }
}
