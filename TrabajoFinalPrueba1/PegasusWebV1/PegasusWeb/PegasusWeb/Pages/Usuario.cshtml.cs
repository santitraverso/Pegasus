using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class UsuarioModel : PageModel
    {
        static HttpClient client = new HttpClient();

        public List<Usuario> Usuarios { get; set; }

        [TempData]
        public int IdUsuario { get; set; }


        public async Task OnGetAsync()
        {
            Usuarios = await GetUsuariosAsync();
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
