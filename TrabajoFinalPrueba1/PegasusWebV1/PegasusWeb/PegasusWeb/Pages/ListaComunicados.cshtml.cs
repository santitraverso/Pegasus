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
        public int IdUsuario { get; set; }

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
                var comunicadosAlumno = await GetComunicadosAlumnosAsync(id);

                var comunicadosAlumnos = new List<ComunicadoAlumnos>();
                //Necesito mostrar todos los usuarios que tiene el comunicado, independientemente del alumno seleccionado
                foreach (var comunicado in comunicadosAlumno)
                {
                    var comuAlumnos = await GetAlumnosComunicadoAsync(comunicado.Id_Comunicado);
                    comunicadosAlumnos.AddRange(comuAlumnos);
                }

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
                .Where(kv => kv.Value.FirstOrDefault()?.Comunicado?.Id_Curso == IdCurso)
                .Select(kv => new ComunicadoViewModel
                {
                    Ids = string.Join(", ", kv.Value.Select(c => c.Id).Distinct()),
                    Id_Comunicado = kv.Key,
                    Descripcion = kv.Value.FirstOrDefault()?.Comunicado?.Descripcion,
                    AlumnosConcatenados = string.Join(", ", kv.Value.Select(c => c.Alumno.Apellido + ' ' + c.Alumno.Nombre).Distinct()),
                    Fecha = kv.Value.FirstOrDefault()?.Comunicado?.Fecha
                })
                .OrderByDescending(c => c.Fecha)
                .ToList();
        }

        private async Task<List<ComunicadoAlumnos>> GetAlumnosComunicadoAsync(int idComunicado)
        {
            List<ComunicadoAlumnos> getusuarios = new List<ComunicadoAlumnos>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_comunicado == {idComunicado}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/ComunicadoAlumnos/GetComunicadoAlumnossForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string usuariosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(usuariosJson))
                {
                    getusuarios = JsonConvert.DeserializeObject<List<ComunicadoAlumnos>>(usuariosJson);
                }
            }

            return getusuarios;
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

        public IActionResult OnPostAtras(int curso, string modulo, int usuario)
        {
            IdCurso = curso;
            Modulo = modulo;
            IdUsuario = usuario;
            return RedirectToPage("Cuaderno");
        }

        public async Task<IActionResult> OnPostAsync(int comunicado, string ids, int curso, int usuario, bool editar, string modulo, string idsAlumnos)
        {
            IdComunicado = comunicado;
            IdCurso = curso;
            IdUsuario = usuario;
            Modulo = modulo;
            IdsAlumnosJson = idsAlumnos;

            if (editar)
            {
                return RedirectToPage("CreateComunicado");
            }
            else
            {
                string trimmedIds = ids.Trim('[', ']');
                string[] idsArray = trimmedIds.Split(',');
                List<int> idsCom = idsArray.Select(id => int.Parse(id)).ToList();

                foreach (var id in idsCom)
                {
                    await EliminarComunicadoAlumnosAsync(id);
                }

                await EliminarComunicadoAsync(IdComunicado);
                if (this.ModelState.IsValid)
                    return RedirectToPage("ListaComunicados");
                else
                    await OnGetAsync();
                    return Page();
            }
        }

        public async Task EliminarComunicadoAsync(int comunicado)
        {
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/CuadernoComunicados/DeleteCuadernoComunicados?id={comunicado}");

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
