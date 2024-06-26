using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("CALIFICACION")]
    public class Calificaciones
    {
        public int Id { get; set; }

        [ForeignKey("Id_Materia")]
        public Materia? Materia { get; set; }
        public int? Id_Materia { get; set; }

        [ForeignKey("Id_Alumno")]
        public Usuario? Usuario { get; set; }
        public int? Id_Alumno { get; set; }

        public int Calificacion { get; set; }
    }
}
