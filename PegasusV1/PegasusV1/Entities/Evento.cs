using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    public class Evento
    {
        public int? Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? Fecha { get; set; }
        [Column("Requiere_Confirmacion")]
        public bool RequiereConfirmacion { get; set; }
        [Column("TIPO_DESTINATARIO")]
        public TipoDestinatario TipoDestinatario { get; set; }
    }
}
