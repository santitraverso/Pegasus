using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("Tareas")]
    public class Tarea
    {
        public int Id { get; set; }

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public DateTime? Fecha_Entrega { get; set; }

        public bool? Entregado { get; set; }

        public double? Calificacion { get; set;}

        [NotMapped]
        [ForeignKey("Id_Materia")]
        public Materia Materia { get; set; }
        public int? Id_Materia { get; set; }

        [NotMapped]
        [ForeignKey("Id_Alumno")]
        public Usuario Alumno { get; set; }
        public int? Id_Alumno { get; set; }
    }
}
