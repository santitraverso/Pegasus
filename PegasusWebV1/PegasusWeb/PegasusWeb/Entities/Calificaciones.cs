using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Calificaciones
    {
        [Key]
        public int Id { get; set; }

        public int? Id_Alumno { get; set; }

        public int? Id_Materia { get; set; }

        public Materia? Materia { get; set; }

        public Usuario? Usuario { get; set; }

        public Byte Calificacion { get; set; }
    }
}

