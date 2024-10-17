using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("DESEMPENIO")]
    public class Desempenio
    {
        public int Id { get; set; }
        public decimal PromedioMin { get; set; } 
        public decimal PromedioMax { get; set; }
        public string Descripcion { get; set; }
    }
}
