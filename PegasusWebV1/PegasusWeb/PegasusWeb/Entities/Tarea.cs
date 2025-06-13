using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Tarea
    {
        public int Id { get; set; }

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public DateTime? Fecha_Entrega { get; set; }

        public bool? Entregado { get; set; }

        public double? Calificacion { get; set;}

        public Materia? Materia { get; set; }
        public int? Id_Materia { get; set; }

        public Usuario? Alumno { get; set; }
        public int? Id_Alumno { get; set; }
    }
}
