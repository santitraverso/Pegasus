using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("DESEMPENO")]
    public class Desempeno
    {
        public int Id { get; set; }
        public decimal PromedioMin { get; set; } 
        public decimal PromedioMax { get; set; }
        public string Descripcion { get; set; }
    }
}
