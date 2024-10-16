using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class ComunicadoAlumnos
    {
        public int Id { get; set; }
        public int? Id_Comunicado { get; set; }
        public CuadernoComunicados? Comunicado { get; set; }
        public int? Id_Alumno { get; set; }
        public Usuario? Alumno { get; set; }
    }
}
