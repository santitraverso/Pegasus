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
        public Curso? Curso { get; set; }

        [Display(Name = "ID_Curso")]
        public int? ID_Curso { get; set; }
    }
}
