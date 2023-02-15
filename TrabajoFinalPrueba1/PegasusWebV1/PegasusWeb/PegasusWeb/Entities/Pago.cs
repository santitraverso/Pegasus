using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Pago
    {
        public int Id { get; set; }

        public Usuario? Alumno { get; set; }
        public int? Id_Alumno { get; set; }

        public string Concepto { get; set; }

        public double? Monto  { get; set; }   

        public bool? Pagado { get; set; }

        public DateTime? Vencimiento { get; set; }
    }
}
