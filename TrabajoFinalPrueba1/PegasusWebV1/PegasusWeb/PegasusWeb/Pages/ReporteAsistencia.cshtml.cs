using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Dynamic;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ReporteAsistenciaModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Asistencia> Alumnos { get; set; } = new List<Asistencia>();

        [TempData]
        public int Materia { get; set; }
        [TempData]
        public DateTime Fecha { get; set; }
        [TempData]
        public int IdCurso { get; set; }
        [TempData]
        public string Modulo { get; set; }

        public async Task OnGetAsync()
        {
             Alumnos = await GetAsistenciaAsync(Materia, Fecha);
        }

        static async Task<List<Asistencia>> GetAsistenciaAsync(int materia, DateTime fecha)
        {
            List<Asistencia> getalumnos = new List<Asistencia>();

            //string fechaString = fecha.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            //var fecha2 = new DateTime(2024, 9, 24);

            //string queryParam = Uri.EscapeDataString($"x=>x.id_Materia=={materia} && x.Fecha.Value.Date == new DateTime({fecha2.Year}, {fecha2.Month}, {fecha2.Day}).Date");
            string queryParam = Uri.EscapeDataString($"x=>x.id_Materia=={materia}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Asistencia/GetAsistenciasForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string alumnosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnosJson))
                {
                    getalumnos = JsonConvert.DeserializeObject<List<Asistencia>>(alumnosJson);
                }
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}, {responseContent}");
            }

            return getalumnos.Where(a => a.Fecha?.ToShortDateString() == fecha.ToShortDateString()).ToList();
        }

        public IActionResult OnPostAtras(int materia, DateTime fecha, int curso, string modulo)
        {
            Materia = materia;
            Fecha = fecha;
            Modulo = modulo;
            IdCurso = curso;
            return RedirectToPage("Asistencia");
        }
    }

}
