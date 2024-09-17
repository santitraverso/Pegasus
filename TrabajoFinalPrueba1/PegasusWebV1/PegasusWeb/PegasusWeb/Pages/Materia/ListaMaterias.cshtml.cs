using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages.Materia
{
    public class ListaMateriasModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<PegasusWeb.Entities.Materia> Materias { get; set; }

        public async Task OnGetAsync()
        {
            Materias = await GetMateriasAsync();
        }

        static async Task<List<PegasusWeb.Entities.Materia>> GetMateriasAsync()
        {
            List<PegasusWeb.Entities.Materia> getmaterias = new List<PegasusWeb.Entities.Materia>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            HttpResponseMessage response = await client.GetAsync("http://localhost:7130/Materia/GetMateriasForCombo");

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
    }
}
