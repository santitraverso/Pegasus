using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Asistencia
    {
        public int Id { get; set; }

        public virtual Usuario? Alumno { get; set; }
        public int? Id_Alumno { get; set; }

        public virtual Materia? Materia { get; set; }
        public int? Id_Materia { get; set; }
        public int? Id_Curso { get; set; }
        public Curso? Curso { get; set; }
        public DateTime? Fecha { get; set; }    

        public bool Presente { get; set; }
    }
}
