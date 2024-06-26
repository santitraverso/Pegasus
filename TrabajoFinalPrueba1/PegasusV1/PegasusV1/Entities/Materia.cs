using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    public class Materia
    {
        public int? Id { get; set; }       
        public string? Nombre { get; set; }
        public int? Id_Curso { get; set; }

        [ForeignKey("Id_Curso")]
        public Curso Curso { get; set; }
    }
}
