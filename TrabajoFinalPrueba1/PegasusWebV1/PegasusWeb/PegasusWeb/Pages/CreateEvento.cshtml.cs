using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Runtime.CompilerServices;
using System.Text;

namespace PegasusWeb.Pages
{
    public class CreateEventoModel : PageModel
    {
        static HttpClient client = new HttpClient();

        public string descripcion;
        public string nombreEvento;
        public string evento;
        public string fecha;

        public void OnGet(int id)
        {
        }

        public async Task<IActionResult> OnPost(string descripcion, string nombreEvento, string fecha)
        {
            if(string.IsNullOrEmpty(descripcion))
            {
                this.ModelState.AddModelError("descripcion", "El campo debe tener valor");
                return null;
            }

            if(string.IsNullOrEmpty(nombreEvento))
            {
                this.ModelState.AddModelError("nombreEvento", "El campo debe tener valor");
                return null;
            }

            if (string.IsNullOrEmpty(fecha))
            {
                this.ModelState.AddModelError("telefono", "El campo debe tener valor");
                return null;
            }

            var content = new StringContent($"{{\"Nombre\":\"{nombreEvento}\", \"Descripcion\":\"{descripcion}\", \"Fecha\":\"{fecha}\"}}", Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("http://localhost:7130/Evento/CreateEvento", content);
            if (!response.IsSuccessStatusCode)
            {
                //Mostrar error de alguna forma
                this.ModelState.AddModelError("evento", "Hubo un error creando el Evento");
                return null;
            }

            return RedirectToPage("Evento");
        }
    }
}
