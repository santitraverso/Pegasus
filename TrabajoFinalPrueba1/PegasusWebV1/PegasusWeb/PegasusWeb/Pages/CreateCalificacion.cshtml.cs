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
        //public IntegrantesMaterias Alumno { get; set; }

        [TempData]
        public int Materia { get; set; }

        [TempData]
        public int Usuario { get; set; }

        [BindProperty]
        public double Nota { get; set; }

        //[BindProperty]
        //public decimal Nota { get; set; }

        //[BindProperty]
        //public int Id { get; set; }


        public void OnPost(int usuario)
        {
            //Alumno = new IntegrantesMaterias { Usuario = new Usuario { Id = id, Apellido = apellido, Nombre = nombre} };

            //Usuario = usuario;
            //Apellido = apellido;
            //Nota = nota;
            //Alumnos = await GetIntegrantesMateriasAsync(materia, usuario);
        }

        public async Task OnGetAsync()
        {
            Alumnos = await GetIntegrantesMateriasAsync(Materia, Usuario);

            if (TempData["Nota"] != null)
            {
                // Recuperar y convertir la string de vuelta a double
                Nota = double.Parse(TempData["Nota"].ToString());
            }
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
