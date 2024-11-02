using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ListaCursosModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<IntegrantesCursos> Cursos { get; set; }

        [TempData]
        public int IdCurso { get; set; }

        [TempData]
        public string Modulo { get; set; }

        [TempData]
        public int IdUsuario { get; set; }

        [TempData]
        public int IdPerfil { get; set; }

        public async Task OnGetAsync()
        {
            if(IdPerfil == 2 || IdPerfil == 4)
            {
                Cursos = await GetCursosAsync(IdUsuario);
            }
            else
            {
                var cursos = await GetCursosAsync();

                Cursos = cursos.GroupBy(ic => ic.Id_Curso).Select(g => g.First()).OrderBy(c => c.Id_Curso).ToList();
            }
            
        }

        static async Task<List<IntegrantesCursos>> GetCursosAsync(int usuario = 0)
        {
            List<IntegrantesCursos> getcursos = new List<IntegrantesCursos>();
            HttpResponseMessage response;

            if (usuario > 0)
            {
                string queryParam = Uri.EscapeDataString($"x=>x.id_usuario=={usuario}");
                response = await client.GetAsync($"https://localhost:7130/IntegrantesCursos/GetIntegrantesCursosForCombo?query={queryParam}");
            }
            else
            {
                response = await client.GetAsync($"https://localhost:7130/IntegrantesCursos/GetIntegrantesCursosForCombo");
            }
           
            if (response.IsSuccessStatusCode)
            {
                string cursosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursosJson))
                {
                    getcursos = JsonConvert.DeserializeObject<List<IntegrantesCursos>>(cursosJson);
                }
            }

            return getcursos;
        }

        public IActionResult OnPost(int curso, string modulo, int usuario, int perfil)
        {
            IdCurso = curso;
            Modulo = modulo;
            IdUsuario = usuario;
            IdPerfil = perfil;

            switch (modulo)
            {
                case "Cuaderno":
                    return RedirectToPage("Cuaderno");
                case "Desempenio":
                    return RedirectToPage("Desempenio");
                default:
                    return RedirectToPage("Materia/ListaMaterias");
            }

        }
    }
}
