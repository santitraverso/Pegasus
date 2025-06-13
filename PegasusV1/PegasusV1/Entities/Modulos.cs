using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("MODULOS")]
    public class Modulos
    {
        public int Id { get; set; }
        public string Modulo { get; set; }

        public string? Parametro { get; set; }

        public string Page { get; set; }
    }
}
