using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class MateriaModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Materia> Materias { get; set; }
        
        public async Task OnGetAsync()
        {
            Materias = await GetMateriasAsync();
        }

        static async Task<List<Materia>> GetMateriasAsync()
        {
            List<Materia> getmaterias = new List<Materia>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            HttpResponseMessage response = await client.GetAsync("https://localhost:7130/Materia/GetMateriasForCombo");

            if (response.IsSuccessStatusCode)
            {
                string materiasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(materiasJson))
                {
                    getmaterias = JsonConvert.DeserializeObject<List<Materia>>(materiasJson);
                }
            }

            return getmaterias;
        }
    }
}
