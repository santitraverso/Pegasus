using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace PegasusWeb.Entities
{
    public class Curso
    {
        [Display(Name = "Id")]
        public int? Id { get; set; }

        [Display(Name = "Nombre")]
        public string? Nombre_Curso { get; set; }

        [Display(Name = "Grado")]
        public byte? Grado { get; set; }

        [Display(Name = "Division")]
        public char? Division { get; set; }

        [Display(Name = "Turno")]
        public string? Turno { get; set; }
    }
}
