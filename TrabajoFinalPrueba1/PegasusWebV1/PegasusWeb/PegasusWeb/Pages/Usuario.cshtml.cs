using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class UsuarioModel : PageModel
    {
        static HttpClient client = new HttpClient();

        public List<Usuario> Usuarios { get; set; }

        [TempData]
        public int IdUsuario { get; set; }
        [TempData]
        public int IdHijo { get; set; }

        [TempData]
        public int IdPerfil { get; set; }


        public async Task OnGetAsync()
        {
            if(IdPerfil == 0)
            {
                IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;
                IdUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            }

            if (IdPerfil == 4)
            {
                IdHijo = HttpContext.Session.GetInt32("IdHijo") ?? 0;
                var hijos = await GetHijosAsync(IdUsuario);
                Usuarios = hijos.Select(h => h.HijoUsuario).ToList();
            }
            else
            {
                Usuarios = await GetUsuariosAsync();
            }
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

        static async Task<List<Usuario>> GetUsuariosAsync()
        {
            List<Usuario> getusuarios = new List<Usuario>();

            HttpResponseMessage response = await client.GetAsync("https://localhost:7130/Usuario/GetUsuariosForCombo");

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

        public static async Task<List<Hijo>> GetHijosAsync(int padre)
        {
            List<Hijo> gethijos = new List<Hijo>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_padre=={padre}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Hijo/GetHijosForCombo?query={queryParam}");

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
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Usuario/DeleteUsuario?id={usuario}");

            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("usuario", "Hubo un error inesperado al borrar el Usuario");
            }
        }
    }
}
