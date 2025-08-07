using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class CuadernoModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public CuadernoModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        public List<IntegrantesCursos> IntegrantesCurso { get; set; } = new List<IntegrantesCursos>();
        [TempData]
        public int IdCurso { get; set; }
        [TempData]
        public string? Modulo { get; set; }

        [BindProperty]
        public List<int> SelectedAlumnosIds { get; set; } = new List<int>(); // IDs de los alumnos seleccionados en el formulario

        [TempData]
        public string? IdsAlumnosJson { get; set; }
        [TempData]
        public int IdComunicado { get; set; }
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
            IdComunicado = 0;

            if (IdPerfil == (int)TipoPerfil.Alumno)
            {
                IntegrantesCurso = await GetIntegrantesCursosAsync(0, IdUsuario);
                IdCurso = (int)IntegrantesCurso.FirstOrDefault().Id_Curso;
            }
            else if (IdPerfil == (int)TipoPerfil.Padre)
            {
                IdUsuario = HttpContext.Session.GetInt32("IdHijo") ?? 0;
                IntegrantesCurso = await GetIntegrantesCursosAsync(0, IdUsuario);
                IdCurso = (int)IntegrantesCurso.FirstOrDefault().Id_Curso;
            }
            else
            {
                IntegrantesCurso = await GetIntegrantesCursosAsync(IdCurso);
            }

            return Page();
        }

        public async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso, int usuario = 0)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();
            string queryParam;

            if (curso != 0)
                queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso}");
            else 
                queryParam = Uri.EscapeDataString($"x=>x.id_usuario=={usuario}");

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/IntegrantesCursos/GetIntegrantesCursosForCombo?query={queryParam}");

            // Añadir el token JWT al encabezado
            string token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

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

        public async Task<IActionResult?> OnPost(int curso, bool ver, string modulo, bool atras, int comunicado, int usuario, int perfil)
        {
            IdCurso = curso;
            Modulo = modulo;
            IdComunicado = comunicado;
            IdUsuario = usuario;
            IdPerfil = perfil;

            if (atras)
            {
                if(IdPerfil == (int)TipoPerfil.Alumno || IdPerfil == (int)TipoPerfil.Padre)
                {
                    return RedirectToPage("Home");
                }
                else
                    return RedirectToPage("ListaCursos");
            }
                

            if (SelectedAlumnosIds.Count < 1)
            {
                this.ModelState.AddModelError("cuaderno", "Debe seleccionar alumnos");
                await OnGetAsync();
                return Page();
            }
            else
            {
                IdsAlumnosJson = JsonConvert.SerializeObject(SelectedAlumnosIds);
            }

            if (ver)
            {
                return RedirectToPage("ListaComunicados");
            }
            else
            {
                return RedirectToPage("CreateComunicado");
            }
          
           
            
        }

        
    }
}
