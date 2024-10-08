using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;

namespace PegasusWeb.Pages
{
    public class CreateMateriaModel : PageModel
    {
        static HttpClient client = new HttpClient();

        [DisplayName("Curso"), Required(ErrorMessage = "El campo Curso es requerido")]
        public int idCurso { get; set; }

        [DisplayName("Nombre de la materia"), Required(ErrorMessage = "El campo Nombre de la materia es requerido")]
        public string nombre { get; set; }

        [DisplayName("Materia"), Required(ErrorMessage = "Hubo un error inesperado creando la Materia")]
        public string materia { get; set; }

        [BindProperty]
        public Entities.Materia Materia { get; set; }

        [TempData]
        public int IdMateria { get; set; }

        public List<IntegrantesCursos> Contenidos { get; set; } = new List<IntegrantesCursos>();

        [BindProperty]
        public int CursoSeleccionadoId { get; set; }

        public List<SelectListItem> CursosRelacionados { get; set; } = new List<SelectListItem> { };

        public async Task<IActionResult> OnGetAsync()
        {

            await CargarCursosAsync();

            if (IdMateria > 0)
            {
                // Es una edici�n, se carga el curso existente
                HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Materia/GetById?id={IdMateria}");

                if (response.IsSuccessStatusCode)
                {
                    string materiaJson = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(materiaJson))
                    {
                        Materia = JsonConvert.DeserializeObject<Entities.Materia>(materiaJson);

                        CursoSeleccionadoId = (int)Materia.Id_Curso;

                        //Alumnos = await GetIntegrantesCursosAsync(IdCurso);
                    }
                }

                if (Materia == null)
                {
                    return NotFound();
                }
            }
            else
            {
                // Es una carga nueva
                Materia = new Entities.Materia { Id = 0 };
                //CursoSeleccionadoId = cursos.FirstOrDefault()?.Id ?? 0;
            }

            return Page();
        }

        static async Task<List<Curso>> GetCursosAsync()
        {
            List<Curso> getcursos = new List<Curso>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Contactos/GetContactosForCombo");
            HttpResponseMessage response = await client.GetAsync("http://localhost:7130/Curso/GetCursosForCombo");
            if (response.IsSuccessStatusCode)
            {
                string cursosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursosJson))
                {
                    getcursos = JsonConvert.DeserializeObject<List<Curso>>(cursosJson);
                }
            }

            return getcursos;
        }

        public IActionResult OnPostModificarContenido(int materia)
        {
            IdMateria = materia;
            return RedirectToPage("ListaContenido");
        }

        static async Task<List<IntegrantesCursos>> GetContenidosMateriaAsync(int materia)
        {
            List<IntegrantesCursos> getContenidos = new List<IntegrantesCursos>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Materia/GetMateriasForCombo");
            string queryParam = Uri.EscapeDataString($"x=>x.id_materia=={materia}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/IntegrantesCursos/GetIntegrantesCursosForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string contenidosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(contenidosJson))
                {
                    getContenidos = JsonConvert.DeserializeObject<List<IntegrantesCursos>>(contenidosJson);
                }
            }

            return getContenidos;
        }

        private async Task CargarCursosAsync()
        {
            var cursos = await GetCursosAsync();

            CursosRelacionados = cursos.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nombre_Curso
            }).ToList();
        }

        public async Task<IActionResult> OnPost(string nombre, int id)
        {
            int idCursoSeleccionado = CursoSeleccionadoId;

            if (idCursoSeleccionado < 1)
            {
                this.ModelState.AddModelError("curso", "El campo Curso es requerido");
            }

            if(string.IsNullOrEmpty(nombre))
            {
                this.ModelState.AddModelError("nombreMateria", "El campo Nombre de la materia es requerido");
            }

            if (!ModelState.IsValid)
            {
                await CargarCursosAsync();
                return Page();
            }


            if (id > 0)
            {
                // Actualizar materia existente
                var content = new StringContent($"{{\"Nombre\":\"{nombre}\", \"Id_Curso\":\"{idCursoSeleccionado}\", \"Id\":\"{id}\"}}", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("http://localhost:7130/Materia/UpdateMateria", content);
                if (!response.IsSuccessStatusCode)
                {
                    this.ModelState.AddModelError("materia", "Hubo un error inesperado al actualizar la Materia.");
                    return null;
                }

            }
            else
            {
                // Crear nueva materia
                var content = new StringContent($"{{\"Nombre\":\"{nombre}\", \"id_Curso\":\"{idCursoSeleccionado}\"}}", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("http://localhost:7130/Materia/CreateMateria", content);
                if (!response.IsSuccessStatusCode)
                {
                    this.ModelState.AddModelError("materia", "Hubo un error inesperado al crear la Materia.");
                }
            }

            return RedirectToPage("../Materia");
        }
    }
}
