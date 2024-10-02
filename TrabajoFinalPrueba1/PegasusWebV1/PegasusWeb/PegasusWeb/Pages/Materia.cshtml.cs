using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class MateriaModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Entities.Materia> Materias { get; set; }

        [TempData]
        public int IdMateria { get; set; }

        public async Task OnGetAsync()
        {
            var materias = await GetMateriasAsync();

            await AsignarCursosAsync(materias);

            Materias = materias;
        }

        private async Task AsignarCursosAsync(List<Entities.Materia> materias)
        {
            foreach (var materia in materias)
            {
                materia.Curso = await GetCursoAsync((int)materia.Id_Curso);
            }
        }

        public async Task<IActionResult> OnPostAsync(int materia, bool editar)
        {
            IdMateria = materia;
   
            if (editar)
            {
                return RedirectToPage("Materia/CreateMateria");
            }
            else
            {
                var tieneAsistencias = await TieneAsistenciasMateria(materia);

                if (tieneAsistencias)
                {
                    ModelState.AddModelError("materia", "No se puede eliminar la materia. Primero elimine las asistencias asociadas.");
                    await OnGetAsync();
                    return Page();
                }

                await EliminarMateriaAsync(materia);
                Materias = await GetMateriasAsync();
                return RedirectToPage("Materia");
            }
        }

        private async Task<bool> TieneAsistenciasMateria(int materia)
        {
            List<Asistencia> getasistencias = new List<Asistencia>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_materia == {materia}");
            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Asistencia/GetAsistenciasForCombo?query={queryParam}");
            if (response.IsSuccessStatusCode)
            {
                string asistenciasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(asistenciasJson))
                {
                    getasistencias = JsonConvert.DeserializeObject<List<Asistencia>>(asistenciasJson);
                }
            }

            return getasistencias.Count > 0;
        }

        static async Task<List<Entities.Materia>> GetMateriasAsync()
        {
            List <Entities.Materia> getMaterias = new List<Entities.Materia>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Contactos/GetContactosForCombo");
            HttpResponseMessage response = await client.GetAsync("http://localhost:7130/Materia/GetMateriasForCombo");
            if (response.IsSuccessStatusCode)
            {
                string materiasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(materiasJson))
                {
                    getMaterias = JsonConvert.DeserializeObject<List<Entities.Materia>>(materiasJson);
                }
            }

            return getMaterias;
        }

        static async Task<Curso> GetCursoAsync(int curso)
        {
            Curso getCurso = new Curso();

            HttpResponseMessage response = await client.GetAsync($"http://localhost:7130/Curso/GetById?id={curso}");
            if (response.IsSuccessStatusCode)
            {
                string cursoJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursoJson))
                {
                    getCurso = JsonConvert.DeserializeObject<Curso>(cursoJson);
                }
            }

            return getCurso;
        }

        public async Task EliminarMateriaAsync(int materia)
        {
            HttpResponseMessage response = await client.DeleteAsync($"http://localhost:7130/Materia/DeleteMateria?id={materia}");

            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("materia", "Hubo un error inesperado al borrar la Materia");
            }
        }
    }
}
