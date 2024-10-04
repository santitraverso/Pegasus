using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
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

        public List<ContenidoMaterias> Contenidos { get; set; } = new List<ContenidoMaterias>();

        [BindProperty]
        public int CursoSeleccionadoId { get; set; }

        public List<SelectListItem> CursosRelacionados { get; set; } = new List<SelectListItem> { };

        public async Task<IActionResult> OnGetAsync()
        {

            //await CargarCursosAsync();

            if (IdMateria > 0)
            {
                // Es una edición, se carga el curso existente
                HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Materia/GetById?id={IdMateria}");

                if (response.IsSuccessStatusCode)
                {
                    string materiaJson = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(materiaJson))
                    {
                        Materia = JsonConvert.DeserializeObject<Entities.Materia>(materiaJson);

                        //CursoSeleccionadoId = (int)Materia.Id_Curso;

                        Contenidos = await GetContenidosMateriaAsync(IdMateria);
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

        //static async Task<List<Curso>> GetCursosAsync()
        //{
        //    List<Curso> getcursos = new List<Curso>();

        //    //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Contactos/GetContactosForCombo");
        //    HttpResponseMessage response = await client.GetAsync("http://localhost:7130/Curso/GetCursosForCombo");
        //    if (response.IsSuccessStatusCode)
        //    {
        //        string cursosJson = await response.Content.ReadAsStringAsync();
        //        if (!string.IsNullOrEmpty(cursosJson))
        //        {
        //            getcursos = JsonConvert.DeserializeObject<List<Curso>>(cursosJson);
        //        }
        //    }

        //    return getcursos;
        //}

        public IActionResult OnPostModificarContenido(int materia)
        {
            IdMateria = materia;
            return RedirectToPage("../ListaContenidos");
        }

        static async Task<List<ContenidoMaterias>> GetContenidosMateriaAsync(int materia)
        {
            List<ContenidoMaterias> getContenidos = new List<ContenidoMaterias>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_materia=={materia}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/ContenidoMaterias/GetContenidoMateriasForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string contenidosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(contenidosJson))
                {
                    getContenidos = JsonConvert.DeserializeObject<List<ContenidoMaterias>>(contenidosJson);
                }
            }

            return getContenidos;
        }

        //private async Task CargarCursosAsync()
        //{
        //    var cursos = await GetCursosAsync();

        //    CursosRelacionados = cursos.Select(c => new SelectListItem
        //    {
        //        Value = c.Id.ToString(),
        //        Text = c.Nombre_Curso
        //    }).ToList();
        //}

        public async Task<IActionResult> OnPost(string nombre, int id, bool atras)
        {
            if (atras)
            {
                IdMateria = id;
                return RedirectToPage("../Materia");
            }
            else
            {
                int idCursoSeleccionado = CursoSeleccionadoId;

                if (idCursoSeleccionado < 1)
                {
                    this.ModelState.AddModelError("curso", "El campo Curso es requerido");
                }

                if (string.IsNullOrEmpty(nombre))
                {
                    this.ModelState.AddModelError("nombreMateria", "El campo Nombre de la materia es requerido");
                }

                if (!ModelState.IsValid)
                {
                    //await CargarCursosAsync();
                    return Page();
                }


                dynamic materiaData = new ExpandoObject();
                materiaData.Nombre = nombre;
                materiaData.Id_Curso = idCursoSeleccionado;

                if (id > 0)
                {
                    materiaData.Id = id;
                }

                // Convertir el objeto dinámico a JSON
                var jsonContent = JsonConvert.SerializeObject(materiaData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
                HttpResponseMessage response;
                if (id > 0)
                {
                    response = await client.PutAsync("http://localhost:7130/Materia/UpdateMateria", content);
                }
                else
                {
                    response = await client.PostAsync("http://localhost:7130/Materia/CreateMateria", content);
                }

                // Manejar errores de la respuesta HTTP
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("materia", id > 0
                        ? "Hubo un error inesperado al actualizar la Materia: " + errorResponse
                        : "Hubo un error inesperado al crear la Materia: " + errorResponse);

                    await OnGetAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = "La Materia se guardó correctamente.";
                return RedirectToPage("../Materia");
            }
        }
    }
}
