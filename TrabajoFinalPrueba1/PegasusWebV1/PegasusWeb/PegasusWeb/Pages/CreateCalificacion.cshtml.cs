using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class CreateCalificacionModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<IntegrantesMaterias> Alumnos { get; set; }
        
        public async Task OnGetAsync(int materia, int usuario)
        {
            Alumnos = await GetIntegrantesMateriasAsync(materia, usuario);
        }

        static async Task<List<IntegrantesMaterias>> GetIntegrantesMateriasAsync(int materia, int usuario)
        {
            List<IntegrantesMaterias> getalumnos = new List<IntegrantesMaterias>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            string queryParam = Uri.EscapeDataString($"x=>x.id_materia = {materia} && x.id_usuario=={usuario}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/IntegrantesMaterias/GetIntegrantesMateriasForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string alumnosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnosJson))
                {
                    getalumnos = JsonConvert.DeserializeObject<List<IntegrantesMaterias>>(alumnosJson);
                }
            }

            return getalumnos;
        }
    }
}
