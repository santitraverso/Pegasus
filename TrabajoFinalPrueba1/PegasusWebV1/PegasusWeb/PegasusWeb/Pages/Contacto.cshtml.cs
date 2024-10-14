using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ContactoModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Contactos> Contactos { get; set; }

        [TempData]
        public int TipoContacto { get; set; }

        [TempData]
        public int IdContacto { get; set; }


        public async Task OnGetAsync()
        {
            Contactos = await GetContactosAsync(TipoContacto);
        }

        static async Task<List<Contactos>> GetContactosAsync(int tipoContacto)
        {
            List<Contactos> getcontactos = new List<Contactos>();

            string queryParam = Uri.EscapeDataString($"x=>x.tipo_contacto=={tipoContacto}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Contactos/GetContactosForCombo?query={queryParam}");
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
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Contactos/DeleteContacto?id={contacto}");

            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("contacto", "Hubo un error inesperado al borrar el Contacto");
            }
        }
    }
}
