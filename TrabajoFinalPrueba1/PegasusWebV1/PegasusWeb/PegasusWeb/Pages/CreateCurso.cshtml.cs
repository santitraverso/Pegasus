using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Dynamic;
using System;

namespace PegasusWeb.Pages
{
    public class CreateCursoModel : PageModel
    {
        static HttpClient client = new HttpClient();

        [BindProperty]
        public Curso Curso { get; set; }

        [TempData]
        public int IdCurso { get; set; }

        public List<IntegrantesCursos> Alumnos { get; set; } = new List<IntegrantesCursos>();
        public List<CursoMateria> Materias { get; set; } = new List<CursoMateria>();

        [BindProperty]
        public List<int> SelectedAlumnosIds { get; set; } = new List<int>(); // IDs de los alumnos seleccionados en el formulario

        [TempData]
        public int IdPerfil { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;

            if (IdCurso > 0)
            {
                Curso = await GetCursoAsync(IdCurso);

                if(Curso != null)
                {
                    Alumnos = await GetIntegrantesCursosAsync(IdCurso);

                    Materias = await GetMateriasCursoAsync(IdCurso);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                Curso = new Curso { Id = 0 };
            }

            return Page();
        }

        public static async Task<Curso> GetCursoAsync(int curso)
        {
            Curso getCurso = new Curso();

            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Curso/GetById?id={curso}");

            if (response.IsSuccessStatusCode)
            {
                string cursoJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursoJson))
                {
                    getCurso = JsonConvert.DeserializeObject<Curso>(cursoJson);
                }
            }

            return getCurso;
        }

        private async Task<List<CursoMateria>> GetMateriasCursoAsync(int curso)
        {
            List<CursoMateria> getMaterias = new List<CursoMateria>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/CursoMateria/GetCursoMateriaForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string materiasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(materiasJson))
                {
                    getMaterias = JsonConvert.DeserializeObject<List<CursoMateria>>(materiasJson);
                }
            }

            return getMaterias;
        }

        public IActionResult OnPostAgregarAlumno(int curso)
        {
            IdCurso = curso;
            return RedirectToPage("IntegrantesCursos");
        }

        public IActionResult OnPostAgregarMateria(int curso)
        {
            IdCurso = curso;
            return RedirectToPage("MateriasCurso");
        }

        static async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            string queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso}");
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

        public async Task<IActionResult> OnPostAsync(bool atras, byte grado, string nombre, char division, string turno, int id)
        {
            if (atras)
            {
                return RedirectToPage("Curso");
            }
            else
            {
                // Validaciones de entrada
                if (grado < 1)
                    ModelState.AddModelError("grado", "El campo Grado es requerido");

                if (string.IsNullOrEmpty(nombre))
                    ModelState.AddModelError("nombre", "El campo Nombre es requerido");

                if (division == '\0')
                    ModelState.AddModelError("division", "El campo Division es requerido");

                if (string.IsNullOrEmpty(turno))
                    ModelState.AddModelError("turno", "El campo Turno es requerido");

                if (!ModelState.IsValid)
                {
                    await OnGetAsync();
                    return Page();
                }

                dynamic cursoData = new ExpandoObject();
                cursoData.Nombre_Curso = nombre;
                cursoData.Grado = grado;
                cursoData.Division = division;
                cursoData.Turno = turno;

                if (id > 0)
                {
                    cursoData.Id = id;
                }

                // Convertir el objeto dinámico a JSON
                var jsonContent = JsonConvert.SerializeObject(cursoData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
                HttpResponseMessage response;
                if (id > 0)
                {
                    response = await client.PutAsync("https://localhost:7130/Curso/UpdateCurso", content);
                }
                else
                {
                    response = await client.PostAsync("https://localhost:7130/Curso/CreateCurso", content);
                }

                // Manejar errores de la respuesta HTTP
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("curso", id > 0
                        ? "Hubo un error inesperado al actualizar el Curso: " + errorResponse
                        : "Hubo un error inesperado al crear el Curso: " + errorResponse);

                    await OnGetAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = "El curso se guardó correctamente.";
                return RedirectToPage("Curso");
            }
        }
    }
}
