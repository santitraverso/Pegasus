using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class CuadernoComunicados
    {
        public int Id { get; set; }        

        public Usuario? Alumno { get; set; }
        public int? Id_Alumno { get; set; }

        public Usuario? Profesor { get; set; }
        public int? Id_Profesor { get; set; }

        public string Descripcion  { get; set; }

        public DateTime? Fecha  { get; set; }
    }
}
