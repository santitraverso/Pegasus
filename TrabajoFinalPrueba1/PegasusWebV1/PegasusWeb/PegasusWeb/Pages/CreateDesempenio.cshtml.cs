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
using System.Globalization;

namespace PegasusWeb.Pages
{
    public class CreateDesempenioModel : PageModel
    {
        static HttpClient client = new HttpClient();

        [BindProperty]
        public DesempenioAlumnos Desempenio { get; set; }

        [TempData]
        public int IdDesempenio { get; set; }

        [TempData]
        public int IdCurso { get; set; }
        [TempData]
        public string Modulo { get; set; }

        [TempData]
        public bool Ver { get; set; }
        [TempData]
        public int IdAlumno { get; set; }

        [BindProperty]
        public Usuario Alumno { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (IdDesempenio > 0)
            {
                Desempenio = await GetDesempenioCursoAsync(IdDesempenio);

                if (Desempenio == null)
                {
                    return NotFound();
                }
            }
            else
            {
                Desempenio = new DesempenioAlumnos { Id = 0 };
            }

            Alumno = await GetUsuarioAsync(IdAlumno);

            return Page();
        }

        private async Task<DesempenioAlumnos> GetDesempenioCursoAsync(int idDesempenio)
        {
            DesempenioAlumnos getusuarios = new DesempenioAlumnos();

            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/DesempenioAlumnos/GetById?id={idDesempenio}");

            if (response.IsSuccessStatusCode)
            {
                string usuariosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(usuariosJson))
                {
                    getusuarios = JsonConvert.DeserializeObject<DesempenioAlumnos>(usuariosJson);
                }
            }

            return getusuarios;
        }

        static async Task<Usuario> GetUsuarioAsync(int usuario)
        {
            Usuario getusuario = new Usuario();

            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Usuario/GetById?id={usuario}");

            if (response.IsSuccessStatusCode)
            {
                string usuarioJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(usuarioJson))
                {
                    getusuario = JsonConvert.DeserializeObject<Usuario>(usuarioJson);
                }
            }

            return getusuario;
        }

        static async Task<CuadernoComunicados> GetComunicadoCursoAsync(int comunicado)
        {
            CuadernoComunicados getcomunicado = new CuadernoComunicados();

            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/CuadernoComunicados/GetById?id={comunicado}");

            if (response.IsSuccessStatusCode)
            {
                string comunicadoJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(comunicadoJson))
                {
                    getcomunicado = JsonConvert.DeserializeObject<CuadernoComunicados>(comunicadoJson);
                }
            }

            return getcomunicado;
        }


        public async Task<IActionResult> OnPostAsync(bool atras, int curso, bool ver, string modulo, int id, string asistencia, string participacion, string calificaciones, string tareas, int alumno)
        {
            IdCurso = curso;
            Modulo = modulo;
            IdDesempenio = id;
            Ver = ver;
            decimal.TryParse(asistencia, NumberStyles.Any, CultureInfo.InvariantCulture, out var asistenciaDecimal);
            decimal.TryParse(participacion, NumberStyles.Any, CultureInfo.InvariantCulture, out var participacionDecimal);
            decimal.TryParse(calificaciones, NumberStyles.Any, CultureInfo.InvariantCulture, out var calificacionesDecimal);
            decimal.TryParse(tareas, NumberStyles.Any, CultureInfo.InvariantCulture, out var tareasDecimal);

            if (atras)
            {
                return RedirectToPage("Desempenio");
            }
            else
            {
                if(ver)
                {
                    await EliminarDesempenioAlumnoAsync(id);
                    if (this.ModelState.IsValid)
                        return RedirectToPage("Desempenio");
                    else
                        await OnGetAsync();
                    return Page();
                }
                else
                {
                    if (asistenciaDecimal < 1 || participacionDecimal < 1 || calificacionesDecimal < 1 || tareasDecimal < 1)
                    {
                        this.ModelState.AddModelError("desempenio", "Todos los campos deben tener un valor.");
                    }

                    if (asistenciaDecimal > 10 || participacionDecimal > 10 || calificacionesDecimal > 10 || tareasDecimal > 10)
                    {
                        this.ModelState.AddModelError("desempenio", "Los campos no pueden tener un valor mayor a 10.");
                    }

                    if (!ModelState.IsValid)
                    {
                        foreach (var state in ModelState)
                        {
                            string fieldName = state.Key; // Nombre del campo que falló
                            var fieldErrors = state.Value.Errors; // Lista de errores asociados al campo

                            foreach (var error in fieldErrors)
                            {
                                Console.WriteLine($"Error en el campo '{fieldName}': {error.ErrorMessage}");
                            }
                        }
                        await OnGetAsync();
                        return Page();
                    }

                    decimal promedio = Math.Round((asistenciaDecimal + participacionDecimal + calificacionesDecimal + tareasDecimal) / 4, 2);

                    dynamic desempenioData = new ExpandoObject();
                    desempenioData.Id_Alumno = alumno;
                    desempenioData.Participacion = participacionDecimal;
                    desempenioData.Asistencia = asistenciaDecimal;
                    desempenioData.Tareas = tareasDecimal;
                    desempenioData.Calificaciones = calificacionesDecimal;
                    desempenioData.Promedio = promedio;
                    desempenioData.Id_Curso = curso;


                    if (id > 0)
                    {
                        desempenioData.Id = id;
                    }

                    // Convertir el objeto dinámico a JSON
                    var jsonContent = JsonConvert.SerializeObject(desempenioData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
                    HttpResponseMessage response;
                    if (id > 0)
                    {
                        response = await client.PutAsync("https://localhost:7130/DesempenioAlumnos/UpdateDesempenioAlumnos", content);
                    }
                    else
                    {
                        response = await client.PostAsync("https://localhost:7130/DesempenioAlumnos/CreateDesempenioAlumnos", content);
                    }

                    // Manejar errores de la respuesta HTTP
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("desempenio", id > 0
                            ? "Hubo un error inesperado al actualizar el Desempenio: " + errorResponse
                            : "Hubo un error inesperado al crear el Desempenio: " + errorResponse);

                        await OnGetAsync();
                        return Page();
                    }

                    TempData["SuccessMessage"] = "El Desempenio se guardó correctamente.";
                    return RedirectToPage("Desempenio");
                }
            }
        }

        public async Task EliminarDesempenioAlumnoAsync(int desempenio)
        {
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/DesempenioAlumnos/DeleteDesempenioAlumnos?id={desempenio}");

            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("desempeno", "Hubo un error inesperado al borrar el Desempeño");
            }
        }
    }
}
