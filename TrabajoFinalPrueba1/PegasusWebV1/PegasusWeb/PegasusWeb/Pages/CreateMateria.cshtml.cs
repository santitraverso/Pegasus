using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Runtime.CompilerServices;
using System.Text;

namespace PegasusWeb.Pages
{
    public class CreateMateriaModel : PageModel
    {
        static HttpClient client = new HttpClient();

        public string curso;
        public string nombreMateria;
        public string materia;

        public void OnGet(int id)
        {
        }

        public async Task<IActionResult> OnPost(string curso, string nombreMateria)
        {
            if(string.IsNullOrEmpty(curso))
            {
                this.ModelState.AddModelError("curso", "El campo debe tener valor");
                return null;
            }

            if(string.IsNullOrEmpty(nombreMateria))
            {
                this.ModelState.AddModelError("nombreMateria", "El campo debe tener valor");
                return null;
            }

            var content = new StringContent($"{{\"Nombre\":\"{nombreMateria}\", \"Curso\":\"{curso}\"}}", Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://pegasus.azure-api.net/v1/Materia/CreateMateria2", content);
            if (!response.IsSuccessStatusCode)
            {
                //Mostrar error de alguna forma
                this.ModelState.AddModelError("materia", "Hubo un error creando la Materia");
                return null;
            }

            return RedirectToPage("Materia");
        }
    }
}
