using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class CalificacionModel : PageModel
    {
        static HttpClient client = new HttpClient();

        public List<IntegrantesMaterias> Alumnos { get; set; }


        [TempData]
        public int Materia { get; set; }
        [TempData]
        public int IdIntegrante { get; set; }
        [TempData]
        public bool Nuevo { get; set; }

        public async Task OnGetAsync()
        {
            Alumnos = await GetIntegrantesMateriasAsync(Materia);
        }

        public IActionResult OnPost(int usuario, bool nuevo)
        {
            IdIntegrante = usuario;
            Nuevo = nuevo;
            return RedirectToPage("CreateCalificacion");
        }

        static async Task<List<IntegrantesMaterias>> GetIntegrantesMateriasAsync(int materia)
        {
            List<IntegrantesMaterias> getalumnos = new List<IntegrantesMaterias>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            string queryParam = Uri.EscapeDataString($"x=>x.id_materia=={materia}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/IntegrantesMaterias/GetIntegrantesMateriasForCombo?query={queryParam}");

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
