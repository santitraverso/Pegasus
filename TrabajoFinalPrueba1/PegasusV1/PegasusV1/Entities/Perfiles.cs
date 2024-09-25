using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("PERFILES")]
    public class Perfiles
    {
        public int Id { get; set; } 
        public string Nombre { get; set; }
    }
}
