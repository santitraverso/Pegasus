using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class DesempenioAlumnos
    {
        public int Id { get; set; }

        public Usuario? Alumno { get; set; }
        public int? Id_Alumno { get; set; }
        public decimal Asistencia { get; set; }
        public decimal Participacion { get; set; }
        public decimal Tareas { get; set; }
        public decimal Calificaciones { get; set; }
        public decimal Promedio { get; set; }
        [NotMapped]
        public Desempenio? Desempenio { get; set; }
    }
}
