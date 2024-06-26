using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;

namespace PegasusWeb.Pages
{
    public class CreateMateriaModel : PageModel
    {
        static HttpClient client = new HttpClient();

        [DisplayName("Curso"), Required(ErrorMessage = "El campo Curso es requerido")]
        public string curso { get; set; }

        [DisplayName("Nombre de la materia"), Required(ErrorMessage = "El campo Nombre de la materia es requerido")]
        public string nombreMateria { get; set; }

        [DisplayName("Materia"), Required(ErrorMessage = "Hubo un error inesperado creando la Materia")]
        public string materia { get; set; }

        public async Task OnGetAsync()
        {
        }

        public async Task<IActionResult> OnPost(string curso, string nombreMateria)
        {
            if(string.IsNullOrEmpty(curso))
            {
                this.ModelState.AddModelError("curso", "El campo Curso es requerido");
                return null;
            }

            if(string.IsNullOrEmpty(nombreMateria))
            {
                this.ModelState.AddModelError("nombreMateria", "El campo Nombre de la materia es requerido");
                return null;
            }

            var content = new StringContent($"{{\"Nombre\":\"{nombreMateria}\", \"Curso\":\"{curso}\"}}", Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://pegasus.azure-api.net/v1/Materia/CreateMateria2", content);
            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("materia", "Hubo un error inesperado al crear la Materia");
                return null;
            }

            return RedirectToPage("Materia");
        }
    }
}
