using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace PegasusWeb.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? ErrorMessage { get; set; }

        public void OnGet(string? message = null)
        {
            ErrorMessage = message ?? "Ha ocurrido un error inesperado."; // Asignar el mensaje recibido o un mensaje predeterminado
        }
    }
}