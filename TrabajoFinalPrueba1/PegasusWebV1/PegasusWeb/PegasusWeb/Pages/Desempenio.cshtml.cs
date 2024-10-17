using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class DesempenioModel : PageModel
    {
        static HttpClient client = new HttpClient();

        public List<DesempenioAlumnos> Alumnos { get; set; } = new List<DesempenioAlumnos>();

        [TempData]
        public int IdDesempeno { get; set; }

        [TempData]
        public int IdCurso { get; set; }

        [TempData]
        public string Modulo { get; set; }

        [BindProperty]
        public List<int> SelectedAlumnosIds { get; set; } = new List<int>();


        public async Task OnGetAsync()
        {
            var integrantes = await GetIntegrantesCursosAsync(IdCurso);

            foreach (var alumn in integrantes)
            {
                DesempenioAlumnos desempeno = new DesempenioAlumnos();
                desempeno.Alumno = alumn.Usuario;
                desempeno.Id_Alumno = alumn.Id_Usuario;

                Alumnos.Add(desempeno);
            }

            var desempenos = await GetDesempenoAlumnosAsync(IdCurso);

            foreach (var alumn in integrantes)
            {
                if (desempenos.Any(i => i.Id_Alumno == alumn.Id_Usuario))
                {
                    SelectedAlumnosIds.Add((int)alumn.Id_Usuario);
                }
            }
        }

        public static async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();

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

        public IActionResult OnPostAtras(int curso, string modulo)
        {
            IdCurso = curso;
            Modulo = modulo;
            return RedirectToPage("ListaCursos");
        }

        public async Task<IActionResult> OnPost(int desempeno, bool editar)
        {
            IdDesempeno = desempeno;

            if (editar)
            {
                return RedirectToPage("CreateDesempeno");
            }
            else
            {
                await EliminarDesempenoAlumnoAsync(desempeno);
                if (this.ModelState.IsValid)
                    return RedirectToPage("Desempeno");
                else
                    await OnGetAsync();
                return Page();
            }
        }

        static async Task<List<DesempenioAlumnos>> GetDesempenoAlumnosAsync(int curso)
        {
            List<DesempenioAlumnos> getusuarios = new List<DesempenioAlumnos>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso == {curso}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/DesempenioAlumnos/GetDesempenioAlumnossForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string usuariosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(usuariosJson))
                {
                    getusuarios = JsonConvert.DeserializeObject<List<DesempenioAlumnos>>(usuariosJson);
                }
            }

            return getusuarios;
        }

        public async Task EliminarDesempenoAlumnoAsync(int desempeno)
        {
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/DesempenioAlumnos/DeleteDesempenioAlumnos?id={desempeno}");

            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("desempeno", "Hubo un error inesperado al borrar el Desempeño");
            }
        }
    }
}
