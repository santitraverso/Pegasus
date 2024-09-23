using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PegasusWeb.Pages
{
    public class CreateUsuarioModel : PageModel
    {
        static HttpClient client = new HttpClient();

        [BindProperty]
        public Usuario Usuario { get; set; }

        [TempData]
        public int IdUsuario { get; set; }

        [BindProperty]
        public int PerfilSeleccionadoId { get; set; }

        public List<SelectListItem> PerfilesRelacionados { get; set; } = new List<SelectListItem> { };

        public async Task<IActionResult> OnGetAsync()
        {
            await CargarPerfilesAsync();

            if (IdUsuario > 0)
            {

                Usuario = await GetUsuarioAsync(IdUsuario);

                if (Usuario == null)
                {
                    return NotFound();
                }

                PerfilSeleccionadoId = (int)Usuario.Perfil;
            }
            else
            {
                Usuario = new Usuario { Id = 0 };
            }

            return Page();
        }

        static async Task<Usuario> GetUsuarioAsync(int usuario)
        {
            Usuario getusuario = new Usuario();

            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Usuario/GetById?id={usuario}");

            if (response.IsSuccessStatusCode)
            {
                string usuarioJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(usuarioJson))
                {
                    getusuario = JsonConvert.DeserializeObject<Usuario>(usuarioJson);
                }
            }

            return getusuario;
        }

        private async Task CargarPerfilesAsync()
        {
            var perfiles = await GetPerfilesAsync();

            PerfilesRelacionados = perfiles.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nombre_Rol
            }).ToList();
        }

        static async Task<List<Roles>> GetPerfilesAsync()
        {
            List<Roles> getperfiles = new List<Roles>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Contactos/GetContactosForCombo");
            HttpResponseMessage response = await client.GetAsync("http://localhost:7130/Roles/GetRolesForCombo");
            if (response.IsSuccessStatusCode)
            {
                string perfilesJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(perfilesJson))
                {
                    getperfiles = JsonConvert.DeserializeObject<List<Roles>>(perfilesJson);
                }
            }

            return getperfiles;
        }


        public async Task<IActionResult> OnPostAsync(bool activo, string nombre, string apellido, string mail, int id)
        {
            int idPerfilSeleccionado = PerfilSeleccionadoId;

            if (idPerfilSeleccionado < 1)
            {
                this.ModelState.AddModelError("perfil", "El campo Perfil es requerido");
            }
            if (string.IsNullOrEmpty(nombre))
            {
                this.ModelState.AddModelError("nombre", "El campo Nombre es requerido");
            }
            if (string.IsNullOrEmpty(apellido))
            {
                this.ModelState.AddModelError("apellido", "El campo Apellido es requerido");
            }
            if (string.IsNullOrEmpty(mail))
            {
                this.ModelState.AddModelError("mail", "El campo Mail es requerido");
            }

            if (!ModelState.IsValid)
            {
                await CargarPerfilesAsync();
                return Page();
            }

            if (id > 0)
            {
                var content = new StringContent($"{{\"perfil\":\"{idPerfilSeleccionado}\", \"activo\":\"{activo}\", \"Apellido\":\"{apellido}\", \"Nombre\":\"{nombre}\", \"mail\":\"{mail}\", \"Id\":\"{id}\"}}", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("http://localhost:7130/Usuario/UpdateUsuario", content);
                if (!response.IsSuccessStatusCode)
                {
                    this.ModelState.AddModelError("usuario", "Hubo un error inesperado al actualizar el Usuario");
                    return null;
                }               
                
            }
            else
            {
                var content = new StringContent($"{{\"perfil\":\"{idPerfilSeleccionado}\", \"activo\":\"{activo}\", \"Apellido\":\"{apellido}\", \"Nombre\":\"{nombre}\", \"mail\":\"{mail}\"}}", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("http://localhost:7130/ContenidoMaterias/CreateUsuario", content);
                if (!response.IsSuccessStatusCode)
                {
                    this.ModelState.AddModelError("usuario", "Hubo un error inesperado al crear el Usuario");
                    return null;
                }
            }

            return RedirectToPage("Usuario");
        }
    }
}
