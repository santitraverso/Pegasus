using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Dynamic;
using Microsoft.Extensions.Options;

namespace PegasusWeb.Pages
{
    public class CreateUsuarioModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public CreateUsuarioModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        [BindProperty]
        public Usuario? Usuario { get; set; }

        [TempData]
        public int IdUsuario { get; set; }

        [BindProperty]
        public int PerfilSeleccionadoId { get; set; }


        public List<SelectListItem> PerfilesRelacionados { get; set; } = new List<SelectListItem> { };

        [BindProperty]
        public List<CursoMateriaPair> CursoMateriaPairs { get; set; } = new List<CursoMateriaPair>();

        public List<SelectListItem> CursosDisponibles { get; set; } = new List<SelectListItem> { };

        public List<SelectListItem> MateriasDisponibles { get; set; } = new List<SelectListItem> { };

        [BindProperty]
        public List<int> HijosSeleccionados { get; set; } = new List<int>();

        public List<Usuario> HijosDisponibles { get; set; } = new List<Usuario>();

        [BindProperty]
        public string? CursoMateriaPairsJson { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await CargarPerfilesAsync();

            if (IdUsuario > 0)
            {

                Usuario = await GetUsuarioAsync(IdUsuario);

                if (Usuario == null)
                {
                    return NotFound();
                }

                PerfilSeleccionadoId = (int)Usuario.Id_Perfil;

                if (PerfilSeleccionadoId == (int)TipoPerfil.Docente)
                {
                    var cursoMateria = await CargarDocenteMateriaAsync(IdUsuario);

                    var cursoMat = cursoMateria.Select(curso => new CursoMateriaPair
                    {
                        CursoId = curso.Id_Curso,
                        MateriaId = curso.Id_Materia
                    });

                    CursoMateriaPairs.AddRange(cursoMat);
                }

                if (PerfilSeleccionadoId == (int)TipoPerfil.Padre)
                {
                    HijosSeleccionados = await CargarHijosPadreAsync(IdUsuario);
                }

            }
            else
            {
                Usuario = new Usuario { Id = 0 };
            }

            await CargarCursosAsync();
            await CargarMateriasAsync();
            await CargarHijosAsync();

            return Page();
        }

        public async Task<List<int>> CargarHijosPadreAsync(int padreId)
        {
            List<int> hijosIds = new List<int>();
            string queryParam = Uri.EscapeDataString($"x=>x.id_padre=={padreId}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Hijo/GetHijosForCombo?query={queryParam}");

            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string hijosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(hijosJson))
                {
                    var hijos = JsonConvert.DeserializeObject<List<Hijo>>(hijosJson);
                    hijosIds = hijos.Where(h => h.Id_Hijo.HasValue).Select(h => h.Id_Hijo.Value).ToList();
                }
            }

            return hijosIds;
        }

        private async Task CargarHijosAsync()
        {
            var hijos = await GetHijosAsync();
            HijosDisponibles = hijos;
        }

        async Task<List<Usuario>> GetHijosAsync()
        {
            List<Usuario> hijos = new List<Usuario>();
            string queryParam = Uri.EscapeDataString($"x=>x.id_perfil=={(int)TipoPerfil.Alumno}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Usuario/GetUsuariosForCombo?query={queryParam}");

            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string hijosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(hijosJson))
                {
                    hijos = JsonConvert.DeserializeObject<List<Usuario>>(hijosJson);
                }
            }
            return hijos;
        }

        public async Task<List<DocenteMateria>> CargarDocenteMateriaAsync(int docente)
        {
            List<DocenteMateria> getcursos = new List<DocenteMateria>();
            string queryParam = Uri.EscapeDataString($"x=>x.id_docente=={docente}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/DocenteMateria/GetDocenteMateriaForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);


            if (response.IsSuccessStatusCode)
            {
                string cursosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursosJson))
                {
                    getcursos = JsonConvert.DeserializeObject<List<DocenteMateria>>(cursosJson);
                }
            }

            return getcursos;
        }

        async Task<Usuario> GetUsuarioAsync(int usuario)
        {
            Usuario getusuario = new Usuario();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Usuario/GetById?id={usuario}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

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

        private async Task CargarPerfilesAsync()
        {
            var perfiles = await GetPerfilesAsync();

            // Obtener el perfil del usuario actual desde la sesión
            var idPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;

            bool isAdmin = false;

            if (idPerfil > 0)
            {
                isAdmin = idPerfil == (int)TipoPerfil.Admin;
            }

            // Filtrar perfiles si el usuario no es admin
            if (!isAdmin)
            {
                perfiles = perfiles.Where(p => p.Id != 1).ToList();
            }

            PerfilesRelacionados = perfiles.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nombre
            }).ToList();
        }

        async Task<List<Perfiles>> GetPerfilesAsync()
        {
            List<Perfiles> getperfiles = new List<Perfiles>();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Perfiles/GetPerfilesForCombo");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string perfilesJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(perfilesJson))
                {
                    getperfiles = JsonConvert.DeserializeObject<List<Perfiles>>(perfilesJson);
                }
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
            }

            return getperfiles;
        }

        async Task<List<Curso>> GetCursosAsync()
        {
            List<Curso> cursos = new List<Curso>();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Curso/GetCursosForCombo");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string cursosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursosJson))
                {
                    cursos = JsonConvert.DeserializeObject<List<Curso>>(cursosJson);
                }
            }
            return cursos;
        }

        async Task<List<Entities.Materia>> GetMateriasAsync()
        {
            List<Entities.Materia> materias = new List<Entities.Materia>();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Materia/GetMateriasForCombo");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string materiasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(materiasJson))
                {
                    materias = JsonConvert.DeserializeObject<List<Entities.Materia>>(materiasJson);
                }
            }
            return materias;
        }

        private async Task CargarCursosAsync()
        {
            var cursos = await GetCursosAsync();

            CursosDisponibles = cursos.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nombre_Curso
            }).ToList();
        }

        private async Task CargarMateriasAsync()
        {
            var materias = await GetMateriasAsync();

            MateriasDisponibles = materias.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Nombre
            }).ToList();
        }


        public async Task<IActionResult> OnPostAsync(bool activo, string nombre, string apellido, string mail, int id, string CursoMateriaPairsJson)
        {
            int idPerfilSeleccionado = PerfilSeleccionadoId;

            // Validaciones
            if (idPerfilSeleccionado < 1)
            {
                this.ModelState.AddModelError("perfil", "El campo Perfil es requerido");
            }
            if (string.IsNullOrEmpty(nombre))
            {
                this.ModelState.AddModelError("nombre", "El campo Nombre es requerido");
            }
            if (string.IsNullOrEmpty(apellido))
            {
                this.ModelState.AddModelError("apellido", "El campo Apellido es requerido");
            }
            if (string.IsNullOrEmpty(mail))
            {
                this.ModelState.AddModelError("mail", "El campo Mail es requerido");
            }

            if (idPerfilSeleccionado == 3 && string.IsNullOrEmpty(CursoMateriaPairsJson))
            {
                this.ModelState.AddModelError("perfil", "El campo Curso y Materia es requerido para un docente");
            }

            if (idPerfilSeleccionado == (int)TipoPerfil.Padre && (HijosSeleccionados == null || !HijosSeleccionados.Any()))
            {
                this.ModelState.AddModelError("hijos", "Debe seleccionar al menos un hijo");
            }

            // Retornar si el modelo no es válido
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            // Obtener el token JWT
            string token = HttpContext.Session.GetString("JwtToken");

            // Crear objeto usuario
            var usuario = new
            {
                Id_Perfil = idPerfilSeleccionado,
                Activo = activo,
                Apellido = apellido,
                Nombre = nombre,
                Mail = mail,
                Id = id > 0 ? id : (int?)null
            };

            var jsonContent = JsonConvert.SerializeObject(usuario);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Hacer el POST o PUT según el caso
            HttpResponseMessage response;
            if (id > 0)
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/Usuario/UpdateUsuario");
                request.Content = content;

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                response = await _client.SendAsync(request);
            }
            else
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/Usuario/CreateUsuario");
                request.Content = content;

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                response = await _client.SendAsync(request);
            }

            // Manejar errores si la solicitud falla
            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("usuario", "Hubo un error inesperado al guardar el Usuario");
                await OnGetAsync();
                return Page();
            }

            // Obtener el ID del usuario si es nuevo
            if (id == 0)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var usuarioCreado = JsonConvert.DeserializeObject<dynamic>(responseContent);
                id = usuarioCreado.id;
            }

            // Borro las posibles relaciones existentes
            if (id > 0)
            {
                await BorrarCursosMateriasAsync(id);
                await BorrarRelacionesPadreHijoAsync(id);
            }

            // Me fijo si es un docente para crear la relación con el curso y la materia 
            if (idPerfilSeleccionado == (int)TipoPerfil.Docente && !string.IsNullOrEmpty(CursoMateriaPairsJson))
            {
                var cursoMateriaPairs = JsonConvert.DeserializeObject<List<CursoMateriaPair>>(CursoMateriaPairsJson);

                if (cursoMateriaPairs != null && cursoMateriaPairs.Any())
                {
                    // Crear una lista de objetos DocenteMateria para todas las relaciones
                    var docenteMateriaList = new List<dynamic>();

                    foreach (var pair in cursoMateriaPairs)
                    {
                        dynamic docenteMateriaData = new ExpandoObject();
                        docenteMateriaData.Id_Docente = id;
                        docenteMateriaData.Id_Materia = pair.MateriaId;
                        docenteMateriaData.Id_Curso = pair.CursoId;

                        docenteMateriaList.Add(docenteMateriaData);
                    }

                    var jsonContentDocenteMateria = JsonConvert.SerializeObject(docenteMateriaList);
                    var contentDocenteMateria = new StringContent(jsonContentDocenteMateria, Encoding.UTF8, "application/json");

                    var requestDocenteMateria = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/DocenteMateria/CreateAllDocenteMateria");
                    requestDocenteMateria.Content = contentDocenteMateria;

                    if (!string.IsNullOrEmpty(token))
                    {
                        requestDocenteMateria.Headers.Add("Authorization", $"Bearer {token}");
                    }

                    var responseDocenteMateria = await _client.SendAsync(requestDocenteMateria);

                    // Manejar errores
                    if (!responseDocenteMateria.IsSuccessStatusCode)
                    {
                        var errorResponse = await responseDocenteMateria.Content.ReadAsStringAsync();
                        ModelState.AddModelError("docente", "Hubo un error inesperado al crear las relaciones entre Docente y Materias: " + errorResponse);
                        await OnGetAsync();
                        return Page();
                    }
                }
            }

            //Para padres
            if (idPerfilSeleccionado == (int)TipoPerfil.Padre && HijosSeleccionados != null && HijosSeleccionados.Any())
            {
                var hijosRelaciones = new List<dynamic>();

                foreach (var hijoId in HijosSeleccionados)
                {
                    dynamic hijoData = new ExpandoObject();
                    hijoData.Id_Padre = id;
                    hijoData.Id_Hijo = hijoId;

                    hijosRelaciones.Add(hijoData);
                }

                var jsonContentHijos = JsonConvert.SerializeObject(hijosRelaciones);
                var contentHijos = new StringContent(jsonContentHijos, Encoding.UTF8, "application/json");

                var requestHijos = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/Hijo/CreateAllHijos");
                requestHijos.Content = contentHijos;

                if (!string.IsNullOrEmpty(token))
                {
                    requestHijos.Headers.Add("Authorization", $"Bearer {token}");
                }

                var responseHijos = await _client.SendAsync(requestHijos);

                if (!responseHijos.IsSuccessStatusCode)
                {
                    var errorResponse = await responseHijos.Content.ReadAsStringAsync();
                    ModelState.AddModelError("hijos", "Hubo un error inesperado al crear las relaciones Padre-Hijo: " + errorResponse);
                    await OnGetAsync();
                    return Page();
                }
            }

            TempData["SuccessMessage"] = "El Usuario se guardó correctamente.";
            return RedirectToPage("Usuario");
        }

        private async Task BorrarCursosMateriasAsync(int docenteId)
        {
            try
            {
                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                string queryParam = Uri.EscapeDataString($"x=>x.id_docente=={docenteId}");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/DocenteMateria/GetDocenteMateriaForCombo?query={queryParam}");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var relaciones = JsonConvert.DeserializeObject<List<DocenteMateria>>(json);

                    if (relaciones != null && relaciones.Any())
                    {
                        var jsonContent = JsonConvert.SerializeObject(relaciones);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/DocenteMateria/DeleteAllDocenteMateria");
                        deleteRequest.Content = content;

                        if (!string.IsNullOrEmpty(token))
                        {
                            deleteRequest.Headers.Add("Authorization", $"Bearer {token}");
                        }

                        await _client.SendAsync(deleteRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error eliminando relaciones docente-materia: {ex.Message}");
            }
        }

        private async Task BorrarRelacionesPadreHijoAsync(int padreId)
        {
            try
            {
                string token = HttpContext.Session.GetString("JwtToken");

                string queryParam = Uri.EscapeDataString($"x=>x.id_padre=={padreId}");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Hijo/GetHijosForCombo?query={queryParam}");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var relaciones = JsonConvert.DeserializeObject<List<Hijo>>(json);

                    if (relaciones != null && relaciones.Any())
                    {
                        var jsonContent = JsonConvert.SerializeObject(relaciones);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/Hijo/DeleteAllHijos");
                        deleteRequest.Content = content;

                        if (!string.IsNullOrEmpty(token))
                        {
                            deleteRequest.Headers.Add("Authorization", $"Bearer {token}");
                        }

                        await _client.SendAsync(deleteRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error eliminando relaciones padre-hijo: {ex.Message}");
            }
        }
    }
}
