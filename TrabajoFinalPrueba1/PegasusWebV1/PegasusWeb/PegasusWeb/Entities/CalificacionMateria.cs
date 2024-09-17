using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class CalificacionMateria
    {
        [Key]
        public int Id { get; set; }

        public int Id_Alumno { get; set; }

        public int Id_Materia { get; set; }

        public int Id_Curso { get; set; }

        public double? Calificacion { get; set; }
    }
}

