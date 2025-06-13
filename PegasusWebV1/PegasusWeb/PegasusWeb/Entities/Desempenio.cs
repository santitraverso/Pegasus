using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Desempenio
    {
        public int Id { get; set; }
        public decimal PromedioMin { get; set; }
        public decimal PromedioMax { get; set; }
        public string? Descripcion { get; set; }
    }
}
