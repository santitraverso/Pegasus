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

                PerfilSeleccionadoId = (int)Usuario.Id_Perfil;
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
                Text = c.Nombre
            }).ToList();
        }

        static async Task<List<Perfiles>> GetPerfilesAsync()
        {
            List<Perfiles> getperfiles = new List<Perfiles>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Contactos/GetContactosForCombo");
            HttpResponseMessage response = await client.GetAsync("http://localhost:7130/Pefiles/GetPefilesForCombo");
            if (response.IsSuccessStatusCode)
            {
                string perfilesJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(perfilesJson))
                {
                    getperfiles = JsonConvert.DeserializeObject<List<Perfiles>>(perfilesJson);
                }
            }
            else
            {
                // Log or inspect the response details if the status is not successful
                Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
            }

            return getperfiles;
        }


        public async Task<IActionResult> OnPostAsync(bool activo, string nombre, string apellido, string mail, int id)
        {
            int idPerfilSeleccionado = PerfilSeleccionadoId;

            // Validaciones
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

            // Retornar si el modelo no es válido
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            // Crear objeto usuario
            var usuario = new
            {
                Perfil = idPerfilSeleccionado,
                Activo = activo,
                Apellido = apellido,
                Nombre = nombre,
                Mail = mail,
                Id = id > 0 ? id : (int?)null // Solo envía el id si es una actualización
            };

            // Convertir el objeto a JSON
            var jsonContent = JsonConvert.SerializeObject(usuario);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Hacer el POST o PUT según el caso
            HttpResponseMessage response;
            if (id > 0)
            {
                response = await client.PutAsync("http://localhost:7130/Usuario/UpdateUsuario", content);
            }
            else
            {
                response = await client.PostAsync("http://localhost:7130/Usuario/CreateUsuario", content);
            }

            // Manejar errores si la solicitud falla
            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("usuario", "Hubo un error inesperado al guardar el Usuario");
                await OnGetAsync();
                return Page();
            }
            TempData["SuccessMessage"] = "El Usuario se guardó correctamente.";
            return RedirectToPage("Usuario");
        }
    }
}
