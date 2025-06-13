using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Perfiles
    {
        [Key]
        public int Id { get; set; } 
        public string Nombre { get; set; }
    }
}
