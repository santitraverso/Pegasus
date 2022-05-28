using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("PAGOS")]
    public class Pago
    {
        public int Id { get; set; }

        [NotMapped]
        [ForeignKey("Id_Alumno")]
        public Usuario Alumno { get; set; }
        public int? Id_Alumno { get; set; }

        public string Concepto { get; set; }

        public double? Monto  { get; set; }   

        public bool? Pagado { get; set; }

        public DateTime? Vencimiento { get; set; }
    }
}
