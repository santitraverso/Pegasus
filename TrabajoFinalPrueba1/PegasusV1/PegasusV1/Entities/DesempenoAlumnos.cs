using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("DESEMPENO_ALUMNOS")]
    public class DesempenoAlumnos
    {
        public int Id { get; set; }
        public int? Id_Alumno { get; set; }
        [ForeignKey("Id_Alumno")]
        public Usuario? Alumno { get; set; }
        public decimal Asistencia { get; set; }
        public decimal Participacion { get; set; }
        public decimal Tareas { get; set; }
        public decimal Calificaciones { get; set; }
        public decimal Promedio { get; set; }
    }
}
