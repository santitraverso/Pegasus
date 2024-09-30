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
using System.Diagnostics.Contracts;
using System.Dynamic;

namespace PegasusWeb.Pages
{
    public class CreateContactoModel : PageModel
    {
        static HttpClient client = new HttpClient();

        [BindProperty]
        public Contactos Contacto { get; set; }

        [TempData]
        public int IdContacto { get; set; }

        [BindProperty]
        public int TipoContactoSeleccionadoId { get; set; }

        [TempData]
        public int TipoContacto { get; set; }


        public List<SelectListItem> TipoContactoRelacionados { get; set; } = new List<SelectListItem> { };

        public async Task<IActionResult> OnGetAsync()
        {
            CargarTipoContactosAsync();

            if (IdContacto > 0)
            {

                var contacto = await GetContactoAsync(IdContacto);

                if (contacto == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(contacto.Nombre))
                {
                    var partes = contacto.Nombre.Split(' ');
                    contacto.NombreDividido = partes[0]; // Primera palabra como nombre

                    // Si hay más de una palabra, considera el resto como apellido
                    contacto.Apellido = partes.Length > 1 ? string.Join(" ", partes.Skip(1)) : "";
                }

                Contacto = contacto;

                TipoContactoSeleccionadoId = (int)Contacto.Tipo_Contacto;
            }
            else
            {
                Contacto = new Contactos { Id = 0 };
            }
            return Page();
        }

        private void CargarTipoContactosAsync()
        {
            TipoContactoRelacionados = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Institucional"},
                new SelectListItem { Value = "2", Text = "Docente"}
            };   
        }

        static async Task<Contactos> GetContactoAsync(int contacto)
        {
            Contactos getcontacto = new Contactos();

            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Contactos/GetById?id={contacto}");

            if (response.IsSuccessStatusCode)
            {
                string contactoJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(contactoJson))
                {
                    getcontacto = JsonConvert.DeserializeObject<Contactos>(contactoJson);
                }
            }

            return getcontacto;
        }

        public async Task<IActionResult> OnPostAsync(string telefono, string nombre, string apellido, string mail, int id)
        {
            int idTipoContactoSeleccionado = TipoContactoSeleccionadoId;

            // Validaciones
            if (idTipoContactoSeleccionado < 1)
            {
                this.ModelState.AddModelError("tipoContacto", "El campo Tipo de Contacto es requerido");
            }
            if (string.IsNullOrEmpty(nombre))
            {
                this.ModelState.AddModelError("nombre", "El campo Nombre es requerido");
            }
            if (string.IsNullOrEmpty(apellido) && idTipoContactoSeleccionado == 2)
            {
                this.ModelState.AddModelError("apellido", "El campo Apellido es requerido");
            }
            if (string.IsNullOrEmpty(mail))
            {
                this.ModelState.AddModelError("mail", "El campo Mail es requerido");
            }
            if (string.IsNullOrEmpty(telefono))
            {
                this.ModelState.AddModelError("telefono", "El campo Telefono es requerido");
            }

            this.ModelState.Remove("apellido");

            // Retornar si el modelo no es válido
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            dynamic contactoData = new ExpandoObject();
            contactoData.Nombre = nombre + ' ' + apellido;
            contactoData.Mail = mail;
            contactoData.telefono = telefono;
            contactoData.Tipo_Contacto = idTipoContactoSeleccionado;

            if (id > 0)
            {
                contactoData.Id = id;
            }

            // Convertir el objeto dinámico a JSON
            var jsonContent = JsonConvert.SerializeObject(contactoData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
            HttpResponseMessage response;
            if (id > 0)
            {
                response = await client.PutAsync("http://localhost:7130/Contactos/UpdateContacto", content);
            }
            else
            {
                response = await client.PostAsync("http://localhost:7130/Contactos/CreateContacto", content);
            }

            // Manejar errores de la respuesta HTTP
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("contacto", id > 0
                    ? "Hubo un error inesperado al actualizar el Contacto: " + errorResponse
                    : "Hubo un error inesperado al crear el Contacto: " + errorResponse);

                await OnGetAsync();
                return Page();
            }

            TipoContacto = TipoContactoSeleccionadoId;
            TempData["SuccessMessage"] = "El contacto se guardó correctamente.";
            return RedirectToPage("Contacto");
        }
    }
}
