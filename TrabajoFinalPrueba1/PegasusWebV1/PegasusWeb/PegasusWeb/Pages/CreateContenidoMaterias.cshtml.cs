using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Dynamic;
using System;

namespace PegasusWeb.Pages
{
    public class CreateContenidoMateriasModel : PageModel
    {
        static HttpClient client = new HttpClient();

        [BindProperty]
        public ContenidoMaterias Contenido { get; set; }

        [TempData]
        public int IdContenido { get; set; }
        [TempData]
        public int IdMateria { get; set; }

        [BindProperty]
        public int Materia { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (IdContenido > 0)
            {

                Contenido = await GetContenidoMateriasAsync(IdContenido);

                if (Contenido == null)
                {
                    return NotFound();
                }
            }
            else
            {
                Contenido = new ContenidoMaterias { Id = 0 };
            }

            Materia = IdMateria;
            return Page();
        }

        static async Task<ContenidoMaterias> GetContenidoMateriasAsync(int contenido)
        {
            ContenidoMaterias getcontenido = new ContenidoMaterias();

            HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/ContenidoMaterias/GetById?id={contenido}");

            if (response.IsSuccessStatusCode)
            {
                string contenidoJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(contenidoJson))
                {
                    getcontenido = JsonConvert.DeserializeObject<ContenidoMaterias>(contenidoJson);
                }
            }

            return getcontenido;
        }


        public async Task<IActionResult> OnPostAsync(bool atras, int materia, string titulo, string descripcion, int id)
        {
            if (atras)
            {
                IdMateria = materia;
                return RedirectToPage("ListaContenidos");
            }
            else
            {
                if (materia < 1)
                {
                    this.ModelState.AddModelError("materia", "El campo Materia es requerido");
                }
                if (string.IsNullOrEmpty(titulo))
                {
                    this.ModelState.AddModelError("titulo", "El campo Titulo es requerido");
                }
                if (string.IsNullOrEmpty(descripcion))
                {
                    this.ModelState.AddModelError("descripcion", "El campo Descripcion es requerido");
                }

                if (!ModelState.IsValid)
                {
                    IdContenido = id;
                    await OnGetAsync();
                    return Page();
                }


                dynamic contenidoData = new ExpandoObject();
                contenidoData.Id_Materia = materia;
                contenidoData.Titulo = titulo;
                contenidoData.Descripcion = descripcion;

                if (id > 0)
                {
                    contenidoData.Id = id;
                }

                // Convertir el objeto dinámico a JSON
                var jsonContent = JsonConvert.SerializeObject(contenidoData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
                HttpResponseMessage response;
                if (id > 0)
                {
                    response = await client.PutAsync("https://localhost:7130/ContenidoMaterias/UpdateContenidoMaterias", content);
                }
                else
                {
                    response = await client.PostAsync("https://localhost:7130/ContenidoMaterias/CreateContenidoMaterias", content);
                }

                // Manejar errores de la respuesta HTTP
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("contenido", id > 0
                        ? "Hubo un error inesperado al actualizar el Contenido: " + errorResponse
                        : "Hubo un error inesperado al crear el Contenido: " + errorResponse);

                    await GetContenidoMateriasAsync(id);
                    return Page();
                }

                IdMateria = materia;
                TempData["SuccessMessage"] = "El Contenido se guardó correctamente.";
                return RedirectToPage("ListaContenidos");
            }
        }
    }
}
