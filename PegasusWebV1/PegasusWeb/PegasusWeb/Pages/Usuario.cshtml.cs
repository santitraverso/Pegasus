using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class UsuarioModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public UsuarioModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<Usuario>? Usuarios { get; set; }

        [TempData]
        public int IdUsuario { get; set; }
        [TempData]
        public int IdHijo { get; set; }

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

            if (IdPerfil == 0)
            {
                IdPerfil = idPerfil;
            }

            if (IdPerfil == (int)TipoPerfil.Padre)
            {
                IdHijo = HttpContext.Session.GetInt32("IdHijo") ?? 0;
                var hijos = await GetHijosAsync(idUsuario); // Usar la variable de la verificación
                Usuarios = hijos.Select(h => h.HijoUsuario).ToList();
            }
            else if (IdPerfil == (int)TipoPerfil.Preceptor)
            {
                Usuarios = await GetUsuariosAsync("x=>x.id_perfil!=1");
            }
            else
                Usuarios = await GetUsuariosAsync();

            return Page();
        }

        public async Task<IActionResult> OnPost(int usuario, bool editar)
        {
            IdUsuario = usuario;

            if (editar)
            {
                return RedirectToPage("CreateUsuario");
            }
            else
            {
                await EliminarUsuarioAsync(usuario);
                Usuarios = await GetUsuariosAsync();
                return RedirectToPage("Usuario");
            }
        }

        public IActionResult OnPostSeleccionarHijo(int hijo)
        {
            HttpContext.Session.SetInt32("IdHijo", hijo);
            return RedirectToPage("Home");
        }

        async Task<List<Usuario>> GetUsuariosAsync(string? query = null)
        {
            List<Usuario> getusuarios = new List<Usuario>();

            string url = $"{_apiBaseUrl}/Usuario/GetUsuariosForCombo";
            if (!string.IsNullOrEmpty(query))
            {
                url += $"?query={Uri.EscapeDataString(query)}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);

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
                    getusuarios = JsonConvert.DeserializeObject<List<Usuario>>(usuariosJson);
                }
            }

            return getusuarios;
        }

        public async Task<List<Hijo>> GetHijosAsync(int padre)
        {
            List<Hijo> gethijos = new List<Hijo>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_padre=={padre}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Hijo/GetHijosForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string hijosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(hijosJson))
                {
                    gethijos = JsonConvert.DeserializeObject<List<Hijo>>(hijosJson);
                }
            }

            return gethijos;
        }

        public async Task EliminarUsuarioAsync(int usuario)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/Usuario/DeleteUsuario/{usuario}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("usuario", "Hubo un error inesperado al borrar el Usuario");
            }
        }
    }
}
