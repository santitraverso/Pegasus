using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Runtime.CompilerServices;
using System.Text;

namespace PegasusWeb.Pages
{
    public class CreateContactosModel : PageModel
    {
        static HttpClient client = new HttpClient();

        public string mail;
        public string nombreContacto;
        public string contacto;
        public string telefono;

        public void OnGet(int id)
        {
        }

        public async Task<IActionResult> OnPost(string mail, string nombreContacto, string telefono)
        {
            if(string.IsNullOrEmpty(mail))
            {
                this.ModelState.AddModelError("mail", "El campo debe tener valor");
                return null;
            }

            if(string.IsNullOrEmpty(nombreContacto))
            {
                this.ModelState.AddModelError("nombreContacto", "El campo debe tener valor");
                return null;
            }

            if (string.IsNullOrEmpty(telefono))
            {
                this.ModelState.AddModelError("telefono", "El campo debe tener valor");
                return null;
            }

            var content = new StringContent($"{{\"Nombre\":\"{nombreContacto}\", \"Mail\":\"{mail}\", \"Telefono\":\"{telefono}\"}}", Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://pegasus.azure-api.net/v1/Contactos/CreateContacto", content);
            if (!response.IsSuccessStatusCode)
            {
                //Mostrar error de alguna forma
                this.ModelState.AddModelError("contacto", "Hubo un error creando el Contacto");
                return null;
            }

            return RedirectToPage("Contacto");
        }
    }
}
