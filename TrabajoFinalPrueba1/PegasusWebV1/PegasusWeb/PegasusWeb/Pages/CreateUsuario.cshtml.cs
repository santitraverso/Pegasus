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

namespace PegasusWeb.Pages
{
    public class CreateUsuarioModel : PageModel
    {
        static HttpClient client = new HttpClient();

        [BindProperty]
        public Usuario Usuario { get; set; }

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
        public string CursoMateriaPairsJson { get; set; }

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

                if (PerfilSeleccionadoId == 3)
                {
                    var cursoMateria = await CargarDocenteMateriaAsync(IdUsuario);

                    var cursoMat = cursoMateria.Select(curso => new CursoMateriaPair
                    {
                        CursoId = curso.Id_Curso,
                        MateriaId = curso.Id_Materia
                    });

                    CursoMateriaPairs.AddRange(cursoMat);
                }
                
            }
            else
            {
                Usuario = new Usuario { Id = 0 };
            }

            await CargarCursosAsync();
            await CargarMateriasAsync();

            return Page();
        }

        public static async Task<List<DocenteMateria>> CargarDocenteMateriaAsync(int docente)
        {
            List<DocenteMateria> getcursos = new List<DocenteMateria>();
            string queryParam = Uri.EscapeDataString($"x=>x.id_docente=={docente}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/DocenteMateria/GetDocenteMateriaForCombo?query={queryParam}");


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

        static async Task<Usuario> GetUsuarioAsync(int usuario)
        {
            Usuario getusuario = new Usuario();

            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Usuario/GetById?id={usuario}");

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

            PerfilesRelacionados = perfiles.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nombre
            }).ToList();
        }

        static async Task<List<Perfiles>> GetPerfilesAsync()
        {
            List<Perfiles> getperfiles = new List<Perfiles>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Contactos/GetContactosForCombo");
            HttpResponseMessage response = await client.GetAsync("https://localhost:7130/Perfiles/GetPerfilesForCombo");
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
                // Log or inspect the response details if the status is not successful
                Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
            }

            return getperfiles;
        }

        static async Task<List<Curso>> GetCursosAsync()
        {
            List<Curso> cursos = new List<Curso>();
            HttpResponseMessage response = await client.GetAsync("https://localhost:7130/Curso/GetCursosForCombo");
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

        static async Task<List<Entities.Materia>> GetMateriasAsync()
        {
            List<Entities.Materia> materias = new List<Entities.Materia>();
            HttpResponseMessage response = await client.GetAsync("https://localhost:7130/Materia/GetMateriasForCombo");
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

            // Retornar si el modelo no es válido
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            // Crear objeto usuario
            var usuario = new
            {
                Id_Perfil = idPerfilSeleccionado,
                Activo = activo,
                Apellido = apellido,
                Nombre = nombre,
                Mail = mail,
                Id = id > 0 ? id : (int?)null // Solo envía el id si es una actualización
            };

            // Convertir el objeto a JSON
            var jsonContent = JsonConvert.SerializeObject(usuario);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Hacer el POST o PUT según el caso
            HttpResponseMessage response;
            if (id > 0)
            {
                response = await client.PutAsync("https://localhost:7130/Usuario/UpdateUsuario", content);
            }
            else
            {
                response = await client.PostAsync("https://localhost:7130/Usuario/CreateUsuario", content);
            }

            // Manejar errores si la solicitud falla
            if (!response.IsSuccessStatusCode)
            {
                this.ModelState.AddModelError("usuario", "Hubo un error inesperado al guardar el Usuario");
                await OnGetAsync();
                return Page();
            }

            //Borro las posibles relaciones que tenga en Docente Materia
            if(id > 0)
            {
                await BorrarCursosMateriasAsync(id);
            }

            //Me fijo si es un docente para crear la relación con el curso y la materia 
            if (idPerfilSeleccionado == 3)
            {
                var cursoMateriaPairs = JsonConvert.DeserializeObject<List<CursoMateriaPair>>(CursoMateriaPairsJson);

                if (id == 0)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var usuarioCreado = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    id = usuarioCreado.id;
                }

                foreach (var pair in cursoMateriaPairs)
                {
                    dynamic docenteMateriaData = new ExpandoObject();
                    docenteMateriaData.Id_Docente = id;
                    docenteMateriaData.Id_Materia = pair.MateriaId;
                    docenteMateriaData.Id_Curso = pair.CursoId;

                    // Convertir el objeto dinámico a JSON
                    var jsonContentAl = JsonConvert.SerializeObject(docenteMateriaData);
                    var contentAl = new StringContent(jsonContentAl, Encoding.UTF8, "application/json");

                    HttpResponseMessage responseAl = await client.PostAsync("https://localhost:7130/DocenteMateria/CreateDocenteMateria", contentAl);

                    // Manejar errores de la respuesta HTTP
                    if (!responseAl.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("docente", "Hubo un error inesperado al crear la relacion Docente y Materia: " + errorResponse);
                        await OnGetAsync();
                        return Page();
                    }
                }
            }
            

            TempData["SuccessMessage"] = "El Usuario se guardó correctamente.";
            return RedirectToPage("Usuario");
        }

        public async Task BorrarCursosMateriasAsync(int docente)
        {
            List<DocenteMateria> docenteMaterias = new List<DocenteMateria>();

            string queryParam = Uri.EscapeDataString($"x=>x.id_docente=={docente}");
            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/DocenteMateria/GetDocenteMateriaForCombo?query={queryParam}");


            if (response.IsSuccessStatusCode)
            {
                string cursosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(cursosJson))
                {
                    docenteMaterias = JsonConvert.DeserializeObject<List<DocenteMateria>>(cursosJson);
                }
            }

            foreach (var docenteMateria in docenteMaterias)
            {
                HttpResponseMessage response2 = await client.GetAsync($"https://localhost:7130/DocenteMateria/DeleteDocenteMateria?id={docenteMateria.Id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("docente", "Hubo un error inesperado al borrar la relacion Docente y Materia: " + errorResponse);
                }
            }
        }
    }
}
