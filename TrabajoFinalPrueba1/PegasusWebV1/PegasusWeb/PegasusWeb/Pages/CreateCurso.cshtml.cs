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

        public async Task<IActionResult> OnGetAsync()
        {
            if (IdCurso > 0)
            {
                // Es una edición, se carga el curso existente
                HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Curso/GetById?id={IdCurso}");

                if (response.IsSuccessStatusCode)
                {
                    string cursoJson = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(cursoJson))
                    {
                        Curso = JsonConvert.DeserializeObject<Curso>(cursoJson);

                        Alumnos = await GetIntegrantesCursosAsync(IdCurso);
                    }
                }

                if (Curso == null)
                {
                    return NotFound();
                }
            }
            else
            {
                // Es una carga nueva
                Curso = new Curso { Id = 0 };
            }

            return Page();
        }

        public IActionResult OnPostAgregarAlumno(int curso)
        {
            IdCurso = curso;
            return RedirectToPage("IntegrantesCursos");
        }

        static async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            string queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/IntegrantesCursos/GetIntegrantesCursosForCombo?query={queryParam}");

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
                    response = await client.PutAsync("http://localhost:7130/Curso/UpdateCurso", content);
                }
                else
                {
                    response = await client.PostAsync("http://localhost:7130/Curso/CreateCurso", content);
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
