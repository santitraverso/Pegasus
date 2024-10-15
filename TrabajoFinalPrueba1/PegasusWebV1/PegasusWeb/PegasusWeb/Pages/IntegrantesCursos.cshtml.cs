using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class IntegrantesCursosModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<IntegrantesCursos> Alumnos { get; set; } = new List<IntegrantesCursos>();
        [BindProperty]
        public List<IntegrantesCursos> IntegrantesCurso { get; set; } = new List<IntegrantesCursos>();
        [TempData]
        public int IdCurso { get; set; }

        [BindProperty]
        public List<int> SelectedAlumnosIds { get; set; } = new List<int>(); // IDs de los alumnos seleccionados en el formulario


        public async Task OnGetAsync()
        {
            //Traigo todos los usuarios que son alumnos
            var todos = await GetUsuariosAlumnosAsync();

            
            foreach (var alumn in todos)
            {
                IntegrantesCursos alumno = new IntegrantesCursos();
                alumno.Usuario = alumn;
                alumno.Id_Usuario = alumn.Id;
                
                Alumnos.Add(alumno);
            }

            //Traigo los integrantes actuales del curso para marcar en la lista de alumnos
            IntegrantesCurso = await GetIntegrantesCursosAsync(IdCurso);

            foreach (var alumn in Alumnos)
            {
                if (IntegrantesCurso.Any(i => i.Id_Usuario == alumn.Id_Usuario))
                {
                    SelectedAlumnosIds.Add((int)alumn.Id_Usuario);
                }
            }
        }

        public static async Task<List<Usuario>> GetUsuariosAlumnosAsync()
        {
            List<Usuario> getalumnos = new List<Usuario>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            string queryParam = Uri.EscapeDataString("x=>x.id_perfil == 2 && x.activo == true");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Usuario/GetUsuariosForCombo?query={queryParam}");
            if (response.IsSuccessStatusCode)
            {
                string alumnosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnosJson))
                {
                    getalumnos = JsonConvert.DeserializeObject<List<Usuario>>(alumnosJson);
                }
            }

            return getalumnos;
        }

        public static async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            string queryParam = Uri.EscapeDataString($"x=>x.id_curso == {curso}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/IntegrantesCursos/GetIntegrantesCursosForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string alumnosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(alumnosJson))
                {
                    getalumnos = JsonConvert.DeserializeObject<List<IntegrantesCursos>>(alumnosJson);
                }
            }

            return getalumnos;
        }

        public async Task<IActionResult?> OnPost(int curso, bool atras)
        {
            if(atras)
            {
                IdCurso = curso;
                return RedirectToPage("CreateCurso");
            }
            else
            {
                IntegrantesCurso = await GetIntegrantesCursosAsync(curso);
                //Borro los integrantes actuales del curso ya que voy a volver a generarlos con los enviados
                bool correct = await BorrarIntegrantesAsync();
                if (correct)
                {
                    foreach (var alumno in SelectedAlumnosIds)
                    {
                        await GuardarIntegrantesAsync(curso, alumno);
                    }
                }

                TempData["SuccessMessage"] = "Los integrantes se guardaron correctamente.";
                return RedirectToPage("Curso");
            }
        }

        public async Task<bool> BorrarIntegrantesAsync()
        {
            foreach (var alumno in IntegrantesCurso)
            {
                HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/IntegrantesCursos/DeleteIntegrantesCursos?id={alumno.Id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task GuardarIntegrantesAsync(int curso, int idAlumno)
        {
            var content = new StringContent($"{{\"ID_CURSO\":\"{curso}\", \"ID_USUARIO\":\"{idAlumno}\"}}", Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://localhost:7130/IntegrantesCursos/CreateIntegrantesCursos", content);
            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("curso", "Hubo un error inesperado al agregar alumnos al curso");
            }
        }
    }
}
