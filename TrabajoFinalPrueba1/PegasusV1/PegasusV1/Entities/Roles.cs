using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("PERFILES")]
    public class Roles
    {
        public int Id { get; set; } 
        public string Nombre { get; set; }
    }
}
