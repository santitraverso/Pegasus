using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class AlumnoCalificacionesViewModel
    {
        public int ?AlumnoId { get; set; }
        public string ?Nombre { get; set; }
        public string ?Apellido { get; set; }
        public List<Calificaciones> ?Calificaciones { get; set; }
    }
}
