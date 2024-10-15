using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("CUADERNO_COMUNICADOS")]
    public class CuadernoComunicados
    {
        public int Id { get; set; }        

        [ForeignKey("Id_Profesor")]
        public Usuario? Profesor { get; set; }
        public int? Id_Profesor { get; set; }

        public string Descripcion  { get; set; }

        public DateTime? Fecha  { get; set; }
    }
}
