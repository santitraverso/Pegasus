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
        [TempData]
        public int IdPerfil { get; set; }

        [TempData]
        public int IdUsuario { get; set; }

        public async Task OnGetAsync()
        {
            IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;
            IdUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            var alumnos = await GetAsistenciaAsync(Materia, Fecha, IdCurso);

            if (IdPerfil == 2)
                Alumnos = alumnos.Where(alu => alu.Id_Alumno == IdUsuario).ToList();
            else if (IdPerfil == 4)
                Alumnos = alumnos.Where(alu => alu.Id_Alumno == HttpContext.Session.GetInt32("IdHijo")).ToList();
            else
                Alumnos = alumnos;
        }


        static async Task<List<Asistencia>> GetAsistenciaAsync(int materia, DateTime fecha, int curso)
        {
            List<Asistencia> getalumnos = new List<Asistencia>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_Materia=={materia} && x.id_curso=={curso}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Asistencia/GetAsistenciasForCombo?query={queryParam}");

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
