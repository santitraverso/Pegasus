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
        public List<CursoMateria> Materias { get; set; } = new List<CursoMateria> { };

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

        static async Task<List<CursoMateria>> GetMateriasAsync(int curso)
        {
            List<CursoMateria> getmaterias = new List<CursoMateria>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso == {curso}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/CursoMateria/GetCursoMateriaForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string materiasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(materiasJson))
                {
                    getmaterias = JsonConvert.DeserializeObject<List<CursoMateria>>(materiasJson);
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
