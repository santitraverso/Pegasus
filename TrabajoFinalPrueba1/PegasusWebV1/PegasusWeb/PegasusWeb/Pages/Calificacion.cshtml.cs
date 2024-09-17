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
        public List<AlumnoCalificacionesViewModel> Alumnos { get; set; }


        [TempData]
        public int Materia { get; set; }
        [TempData]
        public int Usuario { get; set; }

        [BindProperty]
        public double Nota { get; set; }

        public async Task OnGetAsync()
        {
            var alumnos = await GetIntegrantesMateriasAsync(Materia);

            var calificaciones = await GetCalificacionesAsync(Materia);

            // Crear la lista de ViewModels combinando las dos listas
            Alumnos = alumnos.Select(alumno => new AlumnoCalificacionesViewModel
            {
                AlumnoId = alumno.Id_Usuario,
                Nombre = alumno.Usuario.Nombre,
                Apellido = alumno.Usuario.Apellido,
                Calificaciones = calificaciones
                    .Where(c => c.Id_Alumno == alumno.Id_Usuario && c.Id_Materia == alumno.Id_Materia)
                    //.Select(c => new CalificacionViewModel
                    //{
                    //    Asignatura = c.Asignatura,
                    //    Nota = c.Nota
                    //})
                    .ToList()
            }).ToList();


        }

        public IActionResult OnPost(int usuario, int materia)
        {
            Usuario = usuario;
            Materia = materia;
            TempData["Nota"] = Nota.ToString();
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

        static async Task<List<Calificaciones>> GetCalificacionesAsync(int materia)
        {
            List<Calificaciones> getcalificaciones = new List<Calificaciones>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            string queryParam = Uri.EscapeDataString($"x=>x.id_materia=={materia}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Calificaciones/GetCalificacionesForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string calificacionesJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(calificacionesJson))
                {
                    getcalificaciones = JsonConvert.DeserializeObject<List<Calificaciones>>(calificacionesJson);
                }
            }

            return getcalificaciones;
        }
    }
}
