using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    public class Asistencia
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Id_Alumno")]
        public virtual Usuario? Alumno { get; set; }
        public int? Id_Alumno { get; set; }

        [ForeignKey("Id_Materia")]
        public virtual Materia? Materia { get; set; }
        public int? Id_Materia { get; set; }

        public int? Id_Curso { get; set; }
        [ForeignKey("Id_Curso")]
        public Curso? Curso { get; set; }

        public DateTime? Fecha { get; set; }    

        public bool? Presente { get; set; }
    }
}
