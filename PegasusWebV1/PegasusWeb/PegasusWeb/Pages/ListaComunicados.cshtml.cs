using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ListaComunicadosModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public ListaComunicadosModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<ComunicadoViewModel> ComunicadosConAlumnos { get; set; } = new List<ComunicadoViewModel>();

        [TempData]
        public string? IdsAlumnosJson { get; set; }
        [TempData]
        public int IdComunicado { get; set; }
        [TempData]
        public int IdCurso { get; set; }
        [TempData]
        public string? Modulo { get; set; }
        [TempData]
        public int IdUsuario { get; set; }
        [TempData]
        public int IdPerfil { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar sesión válida
            string token = HttpContext.Session.GetString("JwtToken") ?? "";
            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;

            if (string.IsNullOrEmpty(token) || idUsuario <= 0 || idPerfil <= 0)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Index");
            }

            IdPerfil = idPerfil; 
            IdUsuario = idUsuario;

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

            // Si es alumno o padre se quita los otros alumnos que fueron comunicados
            if (IdPerfil == (int)TipoPerfil.Alumno || IdPerfil == (int)TipoPerfil.Padre)
            {
                foreach (var key in comunicadosDict.Keys.ToList())
                {
                    comunicadosDict[key] = comunicadosDict[key]
                        .Where(c => c.Id_Alumno == IdUsuario)
                        .ToList();

                    // Eliminar comunicados sin alumnos válidos después del filtro
                    if (!comunicadosDict[key].Any())
                    {
                        comunicadosDict.Remove(key);
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

            return Page();
        }

        private async Task<List<ComunicadoAlumnos>> GetAlumnosComunicadoAsync(int idComunicado)
        {
            List<ComunicadoAlumnos> getusuarios = new List<ComunicadoAlumnos>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_comunicado == {idComunicado}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/ComunicadoAlumnos/GetComunicadoAlumnossForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

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

        async Task<List<ComunicadoAlumnos>> GetComunicadosAlumnosAsync(int alumno)
        {
            List<ComunicadoAlumnos> getComunicados = new List<ComunicadoAlumnos>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_alumno=={alumno}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/ComunicadoAlumnos/GetComunicadoAlumnossForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

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
                try
                {
                    // Parsear los IDs de las relaciones Comunicado_Alumnos a eliminar
                    string trimmedIds = ids.Trim('[', ']');
                    string[] idsArray = trimmedIds.Split(',');
                    List<int> idsComunicadoAlumnos = idsArray.Select(id => int.Parse(id)).ToList();

                    // Eliminar las relaciones Comunicado_Alumnos
                    bool relacionesEliminadas = await EliminarComunicadoAlumnosAsync(idsComunicadoAlumnos);

                    if (!relacionesEliminadas)
                    {
                        // Si falla la eliminación de relaciones, detener el proceso
                        await OnGetAsync();
                        return Page();
                    }

                    // Eliminar el comunicado principal
                    bool comunicadoEliminado = await EliminarComunicadoAsync(IdComunicado);

                    if (!comunicadoEliminado)
                    {
                        await OnGetAsync();
                        return Page();
                    }

                    if (ModelState.IsValid)
                    {
                        TempData["SuccessMessage"] = "El comunicado y sus relaciones se eliminaron correctamente.";
                        return RedirectToPage("ListaComunicados");
                    }
                    else
                    {
                        await OnGetAsync();
                        return Page();
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("comunicado", $"Error al eliminar comunicado: {ex.Message}");
                    await OnGetAsync();
                    return Page();
                }
            }
        }

        public async Task<bool> EliminarComunicadoAlumnosAsync(List<int> idsComunicadoAlumnos)
        {
            try
            {
                // Si no hay IDs, no hay nada que eliminar
                if (idsComunicadoAlumnos == null || !idsComunicadoAlumnos.Any())
                {
                    return true;
                }

                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                // Crear objetos ComunicadoAlumnos con solo el ID establecido
                var comunicadoAlumnosList = idsComunicadoAlumnos.Select(id => new ComunicadoAlumnos { Id = id }).ToList();

                var jsonContent = JsonConvert.SerializeObject(comunicadoAlumnosList);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/ComunicadoAlumnos/DeleteAllComunicadoAlumnos");
                request.Content = content;

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage response = await _client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("comunicado", "Error al eliminar relaciones de manera masiva: " + errorResponse);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("comunicado", $"Error al eliminar relaciones de manera masiva: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EliminarComunicadoAsync(int idComunicado)
        {
            try
            {
                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/CuadernoComunicados/DeleteCuadernoComunicados/{idComunicado}");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage response = await _client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("comunicado", "Error al eliminar el comunicado: " + errorResponse);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("comunicado", $"Error al eliminar comunicado: {ex.Message}");
                return false;
            }
        }
    }
}
