using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    [Table("PERFILES")]
    public class Perfiles
    {
        public int Id { get; set; } 
        public string Nombre { get; set; }
    }
}
