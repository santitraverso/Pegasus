using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Materia
    {
        [Display(Name = "Id")]
        public int? Id { get; set; }
        [Display(Name = "Nombre")]
        public string? Nombre { get; set; }
    }
}
