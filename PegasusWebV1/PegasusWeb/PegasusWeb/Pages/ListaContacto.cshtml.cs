using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Text.Json.Serialization;

namespace PegasusWeb.Pages
{
    public class ListaContactoModel : PageModel
    {
        [TempData]
        public int TipoContacto { get; set; }
        [TempData]
        public int IdPerfil { get; set; }

        public async Task OnGetAsync()
        {
            IdPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;
        }

        public async Task<IActionResult> OnPostAsync(int tipoContacto)
        {
            TipoContacto = tipoContacto;
            return RedirectToPage("Contacto");
            
        }
    }
}
