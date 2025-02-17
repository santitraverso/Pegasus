using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class CuadernoModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<IntegrantesCursos> IntegrantesCurso { get; set; } = new List<IntegrantesCursos>();
        [TempData]
        public int IdCurso { get; set; }
        [TempData]
        public string Modulo { get; set; }

        [BindProperty]
        public List<int> SelectedAlumnosIds { get; set; } = new List<int>(); // IDs de los alumnos seleccionados en el formulario

        [TempData]
        public string IdsAlumnosJson { get; set; }
        [TempData]
        public int IdComunicado { get; set; }
        [TempData]
        public int IdUsuario { get; set; }
        [TempData]
        public int IdPerfil { get; set; }

        public async Task OnGetAsync()
        {
            IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;
            IdUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            if (IdPerfil == 2)
            {
                IntegrantesCurso = await GetIntegrantesCursosAsync(IdCurso, IdUsuario);
            }
            else if (IdPerfil == 4)
            {
                IdUsuario = HttpContext.Session.GetInt32("IdHijo") ?? 0;
                IntegrantesCurso = await GetIntegrantesCursosAsync(IdCurso, IdUsuario);
            }
            else
            {
                IntegrantesCurso = await GetIntegrantesCursosAsync(IdCurso);
            }
        }

        public static async Task<List<IntegrantesCursos>> GetIntegrantesCursosAsync(int curso, int usuario = 0)
        {
            List<IntegrantesCursos> getalumnos = new List<IntegrantesCursos>();
            string queryParam;

            if (usuario != 0)
                queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso} && x.id_usuario=={usuario}");
            else
                queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso}");

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

        public async Task<IActionResult?> OnPost(int curso, bool ver, string modulo, bool atras, int comunicado, int usuario)
        {
            IdCurso = curso;
            Modulo = modulo;
            IdComunicado = comunicado;
            IdUsuario = usuario;

            if (atras)
                return RedirectToPage("ListaCursos");

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
