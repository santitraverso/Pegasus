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

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar sesión válida
            string token = HttpContext.Session.GetString("JwtToken") ?? "";
            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;

            if (string.IsNullOrEmpty(token) || idUsuario <= 0 || idPerfil <= 0)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Index");
            }

            IdPerfil = idPerfil;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int tipoContacto)
        {
            TipoContacto = tipoContacto;
            return RedirectToPage("Contacto");
            
        }
    }
}
