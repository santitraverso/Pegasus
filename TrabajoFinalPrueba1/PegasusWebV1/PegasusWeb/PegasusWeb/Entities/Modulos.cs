using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Modulos
    {
        public int Id { get; set; }
        public string Modulo { get; set; }
        public string? Parametro { get; set; }
        public string Page { get; set; }
    }
}
