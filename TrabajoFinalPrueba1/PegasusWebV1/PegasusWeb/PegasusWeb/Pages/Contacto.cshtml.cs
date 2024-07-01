using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ContactoModel : PageModel
    {
        static HttpClient client = new HttpClient();
        public List<Contactos> Contactos { get; set; }
        
        public async Task OnGetAsync()
        {
            Contactos = await GetContactosAsync();
        }

        static async Task<List<Contactos>> GetContactosAsync()
        {
            List<Contactos> getcontactos = new List<Contactos>();

            //HttpResponseMessage response = await client.GetAsync("https://pegasus.azure-api.net/v1/Contactos/GetContactosForCombo");
            HttpResponseMessage response = await client.GetAsync("https://localhost:7130/Contactos/GetContactosForCombo");
            if (response.IsSuccessStatusCode)
            {
                string contactosJson = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(contactosJson))
                {
                    getcontactos = JsonConvert.DeserializeObject<List<Contactos>>(contactosJson);
                }
            }

            return getcontactos;
        }
    }
}
