using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class MateriaModel : PageModel
    {
        static HttpClient client = new HttpClient();
        private List<Entities.Materia> materias = new List<Entities.Materia>();

        public List<PegasusWeb.Entities.Materia> Materias { get => materias; set => materias = value; }

        public async Task OnGetAsync()
        {
            materias = await GetMateriasAsync();
        }

        public async Task<List<PegasusWeb.Entities.Materia>> GetMateriasAsync()
        {
            List<PegasusWeb.Entities.Materia> getmaterias = new List<PegasusWeb.Entities.Materia>();

            HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            if (response.IsSuccessStatusCode)
            {
                string materiasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(materiasJson))
                {
                    getmaterias = JsonConvert.DeserializeObject<List<PegasusWeb.Entities.Materia>>(materiasJson);
                }
            }

            return getmaterias;
        }

        public async Task<IActionResult> OnPost(int itemId)
        {
            HttpResponseMessage response = await client.GetAsync($"https://pegasus.azure-api.net/v1/Materia/DeleteMateria?id={itemId}");
            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("materia", "Hubo un error inesperado al eliminar la Materia");
            }

            return RedirectToPage();
        }

    }
}
