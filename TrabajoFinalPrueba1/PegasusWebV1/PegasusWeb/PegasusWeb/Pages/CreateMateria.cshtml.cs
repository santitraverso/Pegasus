using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text;

namespace PegasusWeb.Pages
{
    public class CreateMateriaModel : PageModel
    {
        static HttpClient client = new HttpClient();

        public Materia materia = new Materia();

        public string Nombre = "";

        public void OnGet()
        {
        }

        public async void OnPost()
        {
            var content = new StringContent("{\"Id_Alumno\":1,\"Alumno\":null,\"Materia\":null,\"fecha\": \"2022-09-12T17:44:50.005Z\",\"Id_Materia\":1,\"Presente\":false}", Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://pegasus.azure-api.net/v1/Materia/CreateMateria", content);
            if (response.IsSuccessStatusCode)
            {

            }
        }
    }
}
