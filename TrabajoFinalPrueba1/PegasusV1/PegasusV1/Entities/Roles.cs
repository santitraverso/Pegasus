using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("ROLES")]
    public class Roles
    {
        public int Id { get; set; } 
        public string Nombre_Rol { get; set; }
    }
}
