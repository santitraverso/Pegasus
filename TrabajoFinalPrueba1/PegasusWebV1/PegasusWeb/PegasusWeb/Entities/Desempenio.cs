using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Desempenio
    {
        public int Id { get; set; }

        public Usuario? Alumno  { get; set; }
        public int? Id_Alumno { get; set; }

        public string Descripcion { get; set; }

        public DateTime? Fecha { get; set; }    
    }
}
