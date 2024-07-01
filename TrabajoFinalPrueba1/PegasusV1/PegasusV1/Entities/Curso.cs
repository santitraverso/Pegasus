using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("CURSOS")]
    public class Curso
    {
        public int Id { get; set; }
        public string Nombre_Curso { get; set; } 
        public int Grado { get; set; }
        public string Division { get; set; }
        public string Turno { get; set; }
    }
}
