using System.ComponentModel.DataAnnotations;

namespace PegasusWeb.Entities
{
    public class Materia
    {
        [Display(Name = "Id")]
        public int? Id { get; set; }
        [Display(Name = "Nombre")]
        public string? Nombre { get; set; }
        [Display(Name = "Curso")]
        public string? Curso { get; set; }
    }
}
