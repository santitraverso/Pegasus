using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages.Materia
{
    public class ListaMateriasModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Entities.Materia> Materias { get; set; } = new List<Entities.Materia> { };

        [TempData]
        public int Materia { get; set; }

        [TempData]
        public int IdCurso { get; set; }

        [TempData]
        public string Modulo { get; set; }

        public async Task OnGetAsync()
        {
            Materias = await GetMateriasAsync(IdCurso);
        }

        static async Task<List<Entities.Materia>> GetMateriasAsync(int curso)
        {
            List<Entities.Materia> getmaterias = new List<Entities.Materia>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso == {curso}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/CursoMateria/CursoMateriaForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string materiasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(materiasJson))
                {
                    getmaterias = JsonConvert.DeserializeObject<List<Entities.Materia>>(materiasJson);
                }
            }

            return getmaterias;
        }

        public async Task<IActionResult> OnPostAsync(int materia, string modulo, int curso)
        {
            Materia = materia;
            Modulo = modulo;
            IdCurso = curso;

            switch (modulo)
            {
                case "Calificacion":
                    return RedirectToPage("../Calificacion");
                case "Asistencia":
                    return RedirectToPage("../Asistencia");
                default: 
                    return RedirectToPage("../Index");
            }
        }
    }
}
