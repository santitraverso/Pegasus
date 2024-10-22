using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class MateriasCursoModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Entities.Materia> Materias { get; set; } = new List<Entities.Materia>();
        [BindProperty]
        public List<CursoMateria> MateriasCurso { get; set; } = new List<CursoMateria>();
        [TempData]
        public int IdCurso { get; set; }

        [BindProperty]
        public List<int> SelectedMateriasIds { get; set; } = new List<int>();


        public async Task OnGetAsync()
        {
            Materias = await GetMateriasAsync();

            //Traigo las materias actuales del curso para marcar en la lista de materias
            MateriasCurso = await GetMateriasCursoAsync(IdCurso);

            foreach (var mat in Materias)
            {
                if (MateriasCurso.Any(i => i.Id_Materia == mat.Id))
                {
                    SelectedMateriasIds.Add((int)mat.Id);
                }
            }
        }

        static async Task<List<Entities.Materia>> GetMateriasAsync()
        {
            List<Entities.Materia> getMaterias = new List<Entities.Materia>();

            HttpResponseMessage response = await client.GetAsync("https://localhost:7130/Materia/GetMateriasForCombo");
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

        private async Task<List<CursoMateria>> GetMateriasCursoAsync(int curso)
        {
            List<CursoMateria> getMaterias = new List<CursoMateria>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_curso=={curso}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/CursoMateria/GetCursoMateriaForCombo?query={queryParam}");

            if (response.IsSuccessStatusCode)
            {
                string materiasJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(materiasJson))
                {
                    getMaterias = JsonConvert.DeserializeObject<List<CursoMateria>>(materiasJson);
                }
            }

            return getMaterias;
        }

        public async Task<IActionResult?> OnPost(int curso, bool atras)
        {
            if(atras)
            {
                IdCurso = curso;
                return RedirectToPage("CreateCurso");
            }
            else
            {
                MateriasCurso = await GetMateriasCursoAsync(curso);
                //Borro las materias actuales del curso ya que voy a volver a generarlas con las enviadas
                bool correct = await BorrarMateriasAsync();
                if (correct)
                {
                    foreach (var materia in SelectedMateriasIds)
                    {
                        await GuardarMateriasAsync(curso, materia);
                    }
                }

                TempData["SuccessMessage"] = "Las materias se guardaron correctamente.";
                return RedirectToPage("Curso");
            }
        }

        public async Task<bool> BorrarMateriasAsync()
        {
            foreach (var materia in MateriasCurso)
            {
                HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/CursoMateria/DeleteCursoMateria?id={materia.Id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task GuardarMateriasAsync(int curso, int materia)
        {
            var content = new StringContent($"{{\"ID_CURSO\":\"{curso}\", \"ID_MATERIA\":\"{materia}\"}}", Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://localhost:7130/CursoMateria/CreateCursoMateria", content);
            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("curso", "Hubo un error inesperado al agregar materias al curso");
            }
        }
    }
}
