using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text;
using System.Reflection;
using System.Dynamic;

namespace PegasusWeb.Pages
{
    public class CreateCalificacionModel : PageModel
    {
        static HttpClient client = new HttpClient();

        public IntegrantesMaterias Alumno { get; set; } =new IntegrantesMaterias();
        public List<Calificaciones> Calificaciones { get; set; } = new List<Calificaciones>();

        [TempData]
        public int IdIntegrante { get; set; }
        [TempData]
        public bool Nuevo { get; set; }
        [TempData]
        public int Materia { get; set; }
        [TempData]
        public int IdCurso { get; set; }
        [TempData]
        public string Modulo { get; set; }


        public async Task OnGetAsync()
        {
            if(IdIntegrante == 0 || Materia == 0)
                RedirectToPage("ListaMaterias");

            //Alumno = await GetIntegranteMateriaAsync(IdIntegrante, Materia);

            //if(Nuevo)
            //    Alumno.Usuario.Calificaciones = new List<Calificaciones> { };

            Alumno = await GenerarAlumno(IdCurso, IdIntegrante, Materia);

            if (Nuevo)
                Alumno.Usuario.Calificaciones = new List<Calificaciones> { };
        }

        private async Task<IntegrantesMaterias> GenerarAlumno(int idCurso, int idIntegrante, int materia)
        {
            var alumno = await GetIntegranteCursoAsync(idCurso, idIntegrante);

            // Crea un nuevo objeto IntegrantesMaterias
            IntegrantesMaterias inte = new IntegrantesMaterias
            {
                Id_Materia = materia,
                Id_Usuario = alumno.Id_Usuario,
                Usuario = alumno.Usuario
            };

            inte.Usuario.Calificaciones = await GetCalificacionesAsync(materia, idCurso, (int)inte.Id_Usuario);

            return inte;
        }

        public static async Task<IntegrantesCursos> GetIntegranteCursoAsync(int curso, int usuario)
        {
            IntegrantesCursos getIntegrante = new IntegrantesCursos();
  
            string queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso} && x.id_usuario=={usuario}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/IntegrantesCursos/GetIntegrantesCursosForCombo?query={queryParam}");
            
            if (response.IsSuccessStatusCode)
            {
                string integranteJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(integranteJson))
                {
                    getIntegrante = JsonConvert.DeserializeObject<List<IntegrantesCursos>>(integranteJson).FirstOrDefault();
                }
            }

            return getIntegrante;
        }

        public static async Task<List<Calificaciones>> GetCalificacionesAsync(int materia, int curso, int usuario)
        {
            List<Calificaciones> getCalificaciones = new List<Calificaciones>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_materia=={materia} && x.id_curso=={curso} && x.id_alumno=={usuario}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Calificaciones/GetCalificacionesForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string alumnosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnosJson))
                {
                    getCalificaciones = JsonConvert.DeserializeObject<List<Calificaciones>>(alumnosJson);
                }
            }

            return getCalificaciones;
        }

        //static async Task<IntegrantesMaterias> GetIntegranteMateriaAsync(int alumno, int materia)
        //{
        //    IntegrantesMaterias getalumno = new IntegrantesMaterias();

        //    string queryParam = Uri.EscapeDataString($"x=>x.id_materia=={materia} && x.id_usuario=={alumno}");
        //    HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/IntegrantesMaterias/GetIntegrantesMateriasForCombo?query={queryParam}");

        //    if (response.IsSuccessStatusCode)
        //    {
        //        string alumnoJson = await response.Content.ReadAsStringAsync();
        //        if (!string.IsNullOrEmpty(alumnoJson))
        //        {
        //            getalumno = JsonConvert.DeserializeObject<List<IntegrantesMaterias>>(alumnoJson).FirstOrDefault();
        //        }
        //    }

        //    return getalumno;
        //}

        public async Task<IActionResult> OnPostAsync(List<Calificaciones> calificaciones, int alumno, int materia, string calificacionesEliminadas, int curso, bool atras, string modulo)
        {
            Materia = materia;
            IdCurso = curso;
            Modulo = modulo;
            IdIntegrante = alumno;

            if (atras)
            {
                return RedirectToPage("Calificacion");
            }
            else
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
                    }

                    dynamic calificacionData = new ExpandoObject();
                    calificacionData.Calificacion = calificacion.Calificacion;
                    calificacionData.Id_Materia = materia;
                    calificacionData.Id_Curso = curso;
                    calificacionData.Id_Alumno = alumno;

                    if (calificacion.Id > 0)
                    {
                        calificacionData.Id = calificacion.Id;
                    }

                    // Convertir el objeto dinámico a JSON
                    var jsonContent = JsonConvert.SerializeObject(calificacionData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
                    HttpResponseMessage response;
                    if (calificacion.Id > 0)
                    {
                        response = await client.PutAsync("https://localhost:7130/Calificaciones/UpdateCalificaciones", content);
                    }
                    else
                    {
                        response = await client.PostAsync("https://localhost:7130/Calificaciones/CreateCalificaciones", content);
                    }

                    // Manejar errores de la respuesta HTTP
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("calificacion", calificacion.Id > 0
                            ? "Hubo un error inesperado al actualizar la Calificacion: " + errorResponse
                            : "Hubo un error inesperado al crear la Calificacion: " + errorResponse);

                        await OnGetAsync();
                        return Page();
                    }
                }

                TempData["SuccessMessage"] = "La calificación se guardó correctamente.";
                return RedirectToPage("Calificacion");
            }
        }

        public async Task BorrarCalificacionesAsync(List<int> eliminadas)
        {
            foreach (var calificacion in eliminadas)
            {
                HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Calificaciones/DeleteCalificaciones?id={calificacion}");

                if (!response.IsSuccessStatusCode)
                {
                    this.ModelState.AddModelError("calificacion", "Hubo un error inesperado al borrar la Calificacion");
                }
            }
        }
    }
}
