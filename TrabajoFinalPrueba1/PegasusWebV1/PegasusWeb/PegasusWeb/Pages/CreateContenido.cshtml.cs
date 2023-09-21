using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Runtime.CompilerServices;
using System.Text;

namespace PegasusWeb.Pages
{
    public class CreateContenidoModel : PageModel
    {
        static HttpClient client = new HttpClient();

        public string nombreContenido;
        public string descripcion;
        public string contenido;

        public void OnGet(int id)
        {
        }

        public async Task<IActionResult> OnPost(string descripcion, string nombreContenido)
        {
            if(string.IsNullOrEmpty(descripcion))
            {
                this.ModelState.AddModelError("descripcion", "El campo debe tener valor");
                return null;
            }

            if(string.IsNullOrEmpty(nombreContenido))
            {
                this.ModelState.AddModelError("nombreContenido", "El campo debe tener valor");
                return null;
            }

            var content = new StringContent($"{{\"Nombre\":\"{nombreContenido}\", \"Descripcion\":\"{descripcion}\"}}", Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://pegasus.azure-api.net/v1/Contenido/CreateContenido", content);
            if (!response.IsSuccessStatusCode)
            {
                //Mostrar error de alguna forma
                this.ModelState.AddModelError("contenido", "Hubo un error creando el Contenido");
                return null;
            }

            return RedirectToPage("Contenido");
        }
    }
}
