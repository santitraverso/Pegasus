using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ListaComunicadosModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<ComunicadoViewModel> ComunicadosConAlumnos { get; set; } = new List<ComunicadoViewModel>();

        [TempData]
        public string IdsAlumnosJson { get; set; }
        [TempData]
        public int IdComunicado { get; set; }
        [TempData]
        public int IdCurso { get; set; }
        [TempData]
        public string Modulo { get; set; }
        [TempData]
        public int IdProfesor { get; set; }

        public async Task OnGetAsync()
        {
            // Deserializar el JSON de los IDs de los alumnos
            List<int> idsAlumnos = new List<int>();
            if (!string.IsNullOrEmpty(IdsAlumnosJson))
            {
                idsAlumnos = JsonConvert.DeserializeObject<List<int>>(IdsAlumnosJson);
            }

            // Diccionario para almacenar los comunicados agrupados por ID
            var comunicadosDict = new Dictionary<int, List<ComunicadoAlumnos>>();

            foreach (var id in idsAlumnos)
            {
                var comunicadosAlumnos = await GetComunicadosAlumnosAsync(id);

                foreach (var comunicadoAlumno in comunicadosAlumnos)
                {
                    // Si el comunicado ya está en el diccionario, agregamos el alumno correspondiente
                    if (comunicadosDict.ContainsKey(comunicadoAlumno.Id_Comunicado))
                    {
                        comunicadosDict[comunicadoAlumno.Id_Comunicado].Add(comunicadoAlumno);
                    }
                    else
                    {
                        // Si no está en el diccionario, lo agregamos con el primer alumno asociado
                        comunicadosDict[comunicadoAlumno.Id_Comunicado] = new List<ComunicadoAlumnos> { comunicadoAlumno };
                    }
                }
            }

            ComunicadosConAlumnos = comunicadosDict
                .Where(kv => kv.Value.FirstOrDefault()?.Comunicado?.Id_Profesor == IdProfesor)
                .Select(kv => new ComunicadoViewModel
                {
                    Id_Comunicado = kv.Key,
                    Descripcion = kv.Value.FirstOrDefault()?.Comunicado?.Descripcion,
                    AlumnosConcatenados = string.Join(", ", kv.Value.Select(c => c.Alumno.Apellido + ' ' + c.Alumno.Nombre).Distinct()),
                    Fecha = kv.Value.FirstOrDefault()?.Comunicado?.Fecha
                })
                .OrderByDescending(c => c.Fecha)
                .ToList();
        }

        static async Task<List<ComunicadoAlumnos>> GetComunicadosAlumnosAsync(int alumno)
        {
            List<ComunicadoAlumnos> getComunicados = new List<ComunicadoAlumnos>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_alumno=={alumno}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/ComunicadoAlumnos/GetComunicadoAlumnossForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string comunicadosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(comunicadosJson))
                {
                    getComunicados = JsonConvert.DeserializeObject<List<ComunicadoAlumnos>>(comunicadosJson);
                }
            }

            return getComunicados;
        }

        public IActionResult OnPostAtras(int curso, string modulo, int profesor)
        {
            IdCurso = curso;
            Modulo = modulo;
            IdProfesor = profesor;
            return RedirectToPage("Cuaderno");
        }

        public async Task<IActionResult> OnPostAsync(int comunicado, int curso, int profesor, bool editar, string modulo, string idsAlumnos)
        {
            IdComunicado = comunicado;
            IdCurso = comunicado;
            IdProfesor = profesor;
            Modulo = modulo;
            IdsAlumnosJson = idsAlumnos;

            if (editar)
            {
                return RedirectToPage("CreateComunicado");
            }
            else
            {
                await EliminarComunicadoAsync(IdComunicado);
                await EliminarComunicadoAlumnosAsync(IdComunicado);
                return RedirectToPage("ListaComunicados");
            }
        }

        public async Task EliminarComunicadoAsync(int comunicado)
        {
            HttpResponseMessage response = await client.DeleteAsync($"https://localhost:7130/CuadernoComunicados/DeleteCuadernoComunicados?id={comunicado}");

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                this.ModelState.AddModelError("comunicado", "Hubo un error inesperado al borrar el Comunicado");
            }
        }

        public async Task EliminarComunicadoAlumnosAsync(int comunicado)
        {
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/ComunicadoAlumnos/DeleteComunicadoAlumnos?id={comunicado}");

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                this.ModelState.AddModelError("comunicado", "Hubo un error inesperado al borrar el Comunicado Alumnos");
            }
        }
    }
}
