using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("DESEMPENIO_ALUMNOS")]
    public class DesempenioAlumnos
    {
        public int Id { get; set; }
        public int? Id_Alumno { get; set; }
        [ForeignKey("Id_Alumno")]
        public Usuario? Alumno { get; set; }
        public int? Id_Curso { get; set; }
        [ForeignKey("Id_Curso")]
        public Curso? Curso { get; set; }
        public decimal Asistencia { get; set; }
        public decimal Participacion { get; set; }
        public decimal Tareas { get; set; }
        public decimal Calificaciones { get; set; }
        public decimal Promedio { get; set; }
        [NotMapped]
        public Desempenio? Desempenio { get; set; }
    }
}
