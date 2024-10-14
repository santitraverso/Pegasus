using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PegasusWeb.Pages
{
    public class CreateEventoModel : PageModel
    {
        static HttpClient client = new HttpClient();

        [BindProperty]
        public Evento Evento { get; set; }

        [TempData]
        public int IdEvento { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (IdEvento > 0)
            {
                // Es una edición, se carga el curso existente
                HttpResponseMessage response = await client.GetAsync($"https://localhost:7130/Evento/GetById?id={IdEvento}");

                if (response.IsSuccessStatusCode)
                {
                    string eventoJson = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(eventoJson))
                    {
                        Evento = JsonConvert.DeserializeObject<Evento>(eventoJson);
                    }
                }

                if (Evento == null)
                {
                    return NotFound();
                }
            }
            else
            {
                // Es una carga nueva
                Evento = new Evento { Id = 0 };
                Evento.Fecha = DateTime.Now;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string nombre, string descripcion, DateTime fecha, int id)
        {
            // Validaciones de entrada
            if (string.IsNullOrEmpty(nombre))
                ModelState.AddModelError("nombre", "El campo Nombre es requerido");

            if (string.IsNullOrEmpty(descripcion))
                ModelState.AddModelError("descripcion", "El campo Descripcion es requerido");

            if (fecha == DateTime.MinValue)
                ModelState.AddModelError("fecha", "El campo Fecha es requerido");

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            dynamic eventoData = new ExpandoObject();
            eventoData.Nombre = nombre;
            eventoData.Descripcion = descripcion;
            eventoData.Fecha = fecha;

            if (id > 0)
            {
                eventoData.Id = id;
            }

            // Convertir el objeto dinámico a JSON
            var jsonContent = JsonConvert.SerializeObject(eventoData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
            HttpResponseMessage response;
            if (id > 0)
            {
                response = await client.PutAsync("https://localhost:7130/Evento/UpdateEvento", content);
            }
            else
            {
                response = await client.PostAsync("https://localhost:7130/Evento/CreateEvento", content);
            }

            // Manejar errores de la respuesta HTTP
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("evento", id > 0
                    ? "Hubo un error inesperado al actualizar el Evento: " + errorResponse
                    : "Hubo un error inesperado al crear el Evento: " + errorResponse);

                await OnGetAsync();
                return Page();
            }

            TempData["SuccessMessage"] = "El evento se guardó correctamente.";
            return RedirectToPage("Evento");
        }
    }
}
