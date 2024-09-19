using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text;
using System.Reflection;

namespace PegasusWeb.Pages
{
    public class CreateCalificacionModel : PageModel
    {
        static HttpClient client = new HttpClient();

        [DisplayName("Nota"), Required(ErrorMessage = "El campo Nota es requerido")]

        public IntegrantesMaterias Alumno { get; set; } =new IntegrantesMaterias();
        public List<Calificaciones> Calificaciones { get; set; } = new List<Calificaciones>();

        [TempData]
        public int IdIntegrante { get; set; }

        [TempData]
        public bool Nuevo { get; set; }

        [TempData]
        public int Materia { get; set; }


        public async Task OnGetAsync()
        {
            Alumno = await GetIntegranteMateriaAsync(IdIntegrante);

            if(Nuevo)
                Alumno.Usuario.Calificaciones = new List<Calificaciones> { };
        }

        static async Task<IntegrantesMaterias> GetIntegranteMateriaAsync(int id)
        {
            IntegrantesMaterias getalumno = new IntegrantesMaterias();

            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/IntegrantesMaterias/GetById?id={id}");

            if (response.IsSuccessStatusCode)
            {
                string alumnoJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnoJson))
                {
                    getalumno = JsonConvert.DeserializeObject<IntegrantesMaterias>(alumnoJson);
                }
            }

            return getalumno;
        }

        public async Task<IActionResult> OnPostAsync(List<Calificaciones> calificaciones, int alumno, int materia, string calificacionesEliminadas, int curso)
        {
            if (!string.IsNullOrEmpty(calificacionesEliminadas))
            {
                var idsEliminados = calificacionesEliminadas.Split(',').Select(int.Parse).ToList();

                await BorrarCalificacionesAsync(idsEliminados);
            }

            foreach (var calificacion in calificaciones)
            {
                if (calificacion.Calificacion < 1 || calificacion.Calificacion > 10)
                {
                    this.ModelState.AddModelError("nota", "El campo Nota es requerido");
                    return null;
                }

                if (calificacion.Id > 0)
                {
                    // Actualizar calificacion existente
                    var content = new StringContent($"{{\"calificacion\":\"{calificacion.Calificacion}\", \"Id_Materia\":\"{materia}\", \"Id_Curso\":\"{curso}\", \"Id_Alumno\":\"{alumno}\", \"Id\":\"{calificacion.Id}\"}}", Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PutAsync("http://localhost:7130/Calificaciones/UpdateCalificaciones", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        this.ModelState.AddModelError("calificacion", "Hubo un error inesperado al actualizar la Calificacion");
                        return null;
                    }

                }
                else
                {
                    // Crear nueva calificacion
                    var content = new StringContent($"{{\"calificacion\":\"{calificacion.Calificacion}\", \"Id_Materia\":\"{materia}\", \"Id_Curso\":\"{curso}\", \"Id_Alumno\":\"{alumno}\"}}", Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("http://localhost:7130/Calificaciones/CreateCalificaciones", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        this.ModelState.AddModelError("calificacion", "Hubo un error inesperado al crear la Calificacion");
                        return null;
                    }
                }
            }

            Materia = materia;
            return RedirectToPage("Calificacion");
        }

        public async Task BorrarCalificacionesAsync(List<int> eliminadas)
        {
            foreach (var calificacion in eliminadas)
            {
                HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Calificaciones/DeleteCalificaciones?id={calificacion}");

                if (!response.IsSuccessStatusCode)
                {
                    this.ModelState.AddModelError("calificacion", "Hubo un error inesperado al borrar la Calificacion");
                }
            }
        }
    }
}
